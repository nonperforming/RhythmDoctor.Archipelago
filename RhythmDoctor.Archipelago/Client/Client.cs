using Newtonsoft.Json.Linq;

namespace RhythmDoctor.Archipelago.Client;

/// <summary>
/// Archipelago client
/// </summary>
internal sealed class Client : IDisposable
{
  internal ArchipelagoSession? Session;
  internal DeathLinkService? DeathLink;

  internal SlotData Slot;
  internal readonly TrapManager TrapManager;

  // ReSharper disable once NullableWarningSuppressionIsUsed
  private string _name = null!;
  private string? _password;
  private bool _connected;
  private bool _appliedPatches;
  private int _itemsProcessed;
  private readonly CancellationTokenSource _cancellationTokenSource;

  internal bool Setup => _itemsProcessed == 0;

  private static readonly string[] DeathLinkMessages =
  [
    " couldn't defibrillate well enough",
    " was defeated by Connectifia abortus",
    " couldn't keep the beat",
    " had to go back to med school",
    " lost their ranked match",
    "'s been waiting for so long",
    " woof woof woof woof woof woof woof", // has been waiting for so long
    " is living with regrets",
    "'s dreams stopped",
    " played Falcon",
    " couldn't jump over the box of beans",
    " hit that \"Don't Save Changes\" again",
    " wishes they could write more, and care less",
  ];

  /// <summary>
  /// Create an Archipelago client.
  /// </summary>
  public Client()
  {
    TrapManager = new TrapManager();
    _cancellationTokenSource = new CancellationTokenSource();
  }

  /// <summary>
  /// Create an Archipelago session.
  /// </summary>
  /// <param name="server">Server to connect to</param>
  /// <returns>Created session</returns>
  private ArchipelagoSession CreateSession(string server)
  {
    Plugin.Logger.LogInfo($"Creating Archipelago session to {server}");
    Session = ArchipelagoSessionFactory.CreateSession(server);
    _connected = true;

    Plugin.Logger.LogDebug("Binding events");
    Session.Socket.ErrorReceived += (
      (__exception, __message) => Plugin.Logger.LogError($"[{nameof(Client)}] Socket error {__exception} - {__message}")
    );
    Session.Socket.SocketClosed += (__reason => _ = AttemptReconnect(__reason));
    Session.MessageLog.OnMessageReceived += (
      __message => Plugin.Logger.LogInfo($"[{nameof(Client)}] Received message \"{__message}\"")
    );
    Session.Items.ItemReceived += ItemReceived;
    Session.DataStorage[Scope.Slot, Persistence.PaigeStaysKey].OnValueChanged += ReplicatePaigeStays;
    Session.DataStorage[Scope.Slot, Persistence.IanDesktopLoginKey].OnValueChanged += ReplicateIansDesktopUnlocked;

    return Session;
  }

  private async Task AttemptReconnect(string disconnectReason)
  {
    int tries = 0;
    int maxTries = Configuration.GetAutoReconnectMaxRetries();

    Plugin.Logger.LogWarning($"[{nameof(Client)}] Disconnected from Archipelago: {disconnectReason}");

    while (true)
    {
      _connected = false;
      if (tries >= maxTries)
      {
        Plugin.Logger.LogFatal(
          $"[{nameof(Client)}] Reached max retries of {maxTries}. "
            + "Considering situation unsalvagable and returning to main menu..."
        );
        // ReSharper disable once NullableWarningSuppressionIsUsed
        Plugin.Client = null!;
        Dispose();
        scnBase.GoToMainMenu(); // caught by UnapplyPatchesPatch
      }

      Plugin.Logger.LogWarning($"[{nameof(Client)}] Attempting to reconnect (try {tries}/{maxTries})");

      try
      {
        LoginResult result = await Task.Run(() => Connect(_name, _password), _cancellationTokenSource.Token);
        if (result is LoginSuccessful)
          break;
        else if (result is LoginFailure)
          Plugin.Logger.LogError($"[{nameof(Client)}] Failed to reconnect: try {tries}/{maxTries}");
        else
          Plugin.Logger.LogError($"[{nameof(Client)}] Failed to reconnect - unknown error: try {tries}/{maxTries}");
      }
      catch (Exception exception)
      {
        Plugin.Logger.LogError($"[{nameof(Client)}] Failed to reconnect - {exception}: try {tries}/{maxTries}");
      }

      tries++;
    }

    _connected = true;
    Plugin.Logger.LogWarning($"[{nameof(Client)}] Reconnected!");
  }

  /// <summary>
  /// Connect to an Archipelago room.
  /// </summary>
  /// <remarks>
  /// This method does not load the Level Select.
  /// </remarks>
  /// <param name="name">Slot name to connect under.</param>
  /// <param name="password">Password of the room.</param>
  /// <exception cref="NullReferenceException">Session is null.</exception>
  /// <exception cref="Exception">Login failure.</exception>
  private async Task<LoginResult> Connect(string name, string? password = null)
  {
    if (Session is null)
      throw new NullReferenceException("Session is null");

    _name = name;
    _password = password;

    Plugin.Logger.LogInfo("Attempting to connect");
    _ = await Task.Run(Session.ConnectAsync, _cancellationTokenSource.Token);
    Plugin.Logger.LogInfo("Connected");

    Plugin.Logger.LogInfo("Attempting to login");
    LoginResult loginResult = await Task.Run(
      () =>
        Session.LoginAsync(
          "Rhythm Doctor",
          name,
          _appliedPatches ? ItemsHandlingFlags.RemoteItems : ItemsHandlingFlags.AllItems,
          new Version("0.6.7"),
          null, // DeathLink is managed by DeathLinkService
          null, // Randomly generated
          password
        ),
      _cancellationTokenSource.Token
    );
    Plugin.Logger.LogInfo("Logged in");

    switch (loginResult)
    {
      case LoginFailure loginFailure:
        Plugin.Logger.LogError("Login failed (Client)");
        for (int i = 0; i < loginFailure.Errors.Length; i++)
        {
          string error = loginFailure.Errors[i];
          ConnectionRefusedError errorCode = loginFailure.ErrorCodes[i];

          Plugin.Logger.LogError($"Error {errorCode}: {error}");
        }

        return loginResult;
      case LoginSuccessful loginSuccessful:
        _connected = true;
        Plugin.Logger.LogInfo(
          $"Successfully logged into {loginSuccessful.Slot}/{name} as {Session.ConnectionInfo.Uuid}"
        );

        if (_appliedPatches)
          return loginResult;

        Configuration.DeathLinkConfig deathLink = await Task.Run(
          Configuration.GetDeathLink,
          _cancellationTokenSource.Token
        );
        Slot = new SlotData(loginSuccessful.SlotData);
        if (
          (deathLink == Configuration.DeathLinkConfig.FollowSlot && Slot.deathLink)
          || deathLink == Configuration.DeathLinkConfig.On
        )
        {
          Plugin.Logger.LogInfo("Creating DeathLink");
          DeathLink = Session.CreateDeathLinkService();
          DeathLink.EnableDeathLink();
          DeathLink.OnDeathLinkReceived += DeathLinkReceived;
        }

        Persistence.currentSlotIndex = 0; // Slot 1
        Plugin.ApplyGameplayPatches();
        _appliedPatches = true;

        // Scary!!!!!!!!!!!
        // Hopefully if we got here without any exceptions SavingPatch should be applied,
        //  so we shouldn't lose our first slot in the case of a crash.
        // When we are quitting, the original data should be reloaded by UnapplyPatchesPatch.
        Persistence.slotPrefs[0].dict.Clear();

        // Let LockSleevePaintPatch set the Sleeve Paint to Slot 1's default
        Persistence.p1Skin.Reload();
        Persistence.p2Skin.Reload();

        // Some levels come unlocked by default, such as X-1.
        // Lock all levels to force the user to unlock them with an item.
        foreach (Level level in Enum.GetValues(typeof(Level)))
        {
          Persistence.SetLevelRank(level, Rank.NotAvailable, true);
        }

        _connected = true;
        return loginResult;
      default:
        string message = $"Unknown error: failed to connect to {name}";
        Plugin.Logger.LogError(message);
        throw new Exception(message);
    }
  }

  private async Task PrepareSlot()
  {
    if (Session is null)
      throw new NullReferenceException("Session is null");
    // TODO: Check if session exists but is invalid

    // Setup DataStorage and TrapManager.ClearedTraps, Sticky Traps,
    // initial Paige stays (this can change!)/Ian's desktop (etc) state
    await Task.Run(StateReplicationPatch.InitializeSync, _cancellationTokenSource.Token);

    foreach (Type trapType in Bindings.Traps)
    {
      ITrap trap = (ITrap)Activator.CreateInstance(trapType);
      // ReSharper disable once NullableWarningSuppressionIsUsed
      Plugin.Client.Session!.DataStorage[Scope.Slot, trap.Name].Initialize(0);
      Plugin.Client.TrapManager.ClearedTraps.Add(trap.Name, 0);
    }
    Plugin.Client.TrapManager.AddStickyTraps(Plugin.Client.Slot.stickyTraps);
  }

  /// <summary>
  /// Create an Archipelago session and connect to a room.
  /// </summary>
  /// <param name="server">Server to connect to</param>
  /// <param name="name">Slot name to connect under</param>
  /// <param name="password">Password of the room</param>
  internal async Task<LoginResult> CreateSessionAndConnect(string server, string name, string? password = null)
  {
    CreateSession(server);
    LoginResult loginResult = await Task.Run(() => Connect(name, password), _cancellationTokenSource.Token);
    if (!loginResult.Successful)
      return loginResult;

    await Task.Run(PrepareSlot, _cancellationTokenSource.Token);
    return loginResult;
  }

  private void ItemReceived(ReceivedItemsHelper helper) =>
    Task.Run(async () => await ProcessItem(helper), _cancellationTokenSource.Token);

  private async Task ProcessItem(ReceivedItemsHelper helper)
  {
    bool IsReady(ItemInfo item)
    {
      if (scnBase.instance is scnCLS)
        return false;
      if (_appliedPatches)
        return true;

      // Process traps as early as possible...
      if (Bindings.TrapItemIdToLevel.Keys.Contains(item.ItemId))
        return true;

      // TODO: Based on login progress, allow more items to be accepted (i.e. levels)
      // Levels require slot setup.

      return false;
    }

    bool queued = false;

    // Wait if we aren't connected - i.e. disconnection just after we get received item packet
    if (!_connected)
    {
      Plugin.Logger.LogWarning($"[{nameof(Client)}] Client not connected, waiting...");
      _itemsProcessed++; // FIXME: different way to implement this please
      while (!_connected)
      {
        await Task.Delay(1000, _cancellationTokenSource.Token);
      }
      Plugin.Logger.LogWarning($"[{nameof(Client)}] Client connected, continuing...");
    }

    ItemInfo item = helper.DequeueItem();

    // Ensure the save is prepared before we attempt to load our existing items.
    if (!IsReady(item))
    {
      // FIXME: This is terrible and should be done in a different way
      queued = true;
      Plugin.Logger.LogDebug($"[{nameof(Client)}] Not ready, waiting...");
      while (!IsReady(item))
      {
        await Task.Delay(1000, _cancellationTokenSource.Token);
      }
      Plugin.Logger.LogDebug($"[{nameof(Client)}] Ready, continuing...");
    }

    if (item.ItemGame != Bindings.GAME)
    {
      Plugin.Logger.LogDebug(
        $"Ignoring item {item.ItemName} ({item.ItemId} from {item.ItemGame}), as it is not for our world"
      );
      _itemsProcessed--;
      return;
    }

    // ReSharper disable NullableWarningSuppressionIsUsed
    if (Bindings.ItemIdToLevel.TryGetValue(item.ItemId, out Level level))
    {
      Plugin.Logger.LogInfo($"[{nameof(Client)}] Unlocking stage item {item.ItemName} ({item.ItemId}, {level})");

      if (queued)
      {
        Plugin.Logger.LogInfo($"Attempting to get rank from locations cleared for {level}");

        // Attempt to get rank from locations sent
        if (level == Level.RhythmWeightlifter)
        {
          // Rhythm Weightlifter is a special case in that it has 10 stages inside its level.
          // As the stages can only be played sequentially, and we don't have any specific Rank locations,
          //  we can take a shortcut and just set the last level unlocked to the number of
          //  Weightlifter locations we have cleared.
          int stagesCleared = Session!.Locations.AllLocationsChecked.Count(locationId =>
            Bindings.RhythmWeightlifterStageToLocationID.Contains(locationId)
          );

          if (stagesCleared == 0)
          {
            // We haven't cleared any stages yet.
          }
          else
          {
            Plugin.Logger.LogInfo($"Unlocking Rhythm Weightlifter stages up to stage {stagesCleared}");
            Persistence.SetRhythmWeightlifterLastLevelUnlocked(stagesCleared);
          }
        }
        else
        {
          BaseStage levelStage = Bindings.LevelToStage[level];

          if (levelStage is RegularStage regularStage)
          {
            if (Session!.Locations.AllLocationsChecked.Contains(regularStage.SRankLocation))
            {
              Plugin.Logger.LogInfo("Found S rank location");
              Persistence.SetLevelRank(level, Rank.S, false, false);
            }
            else if (
              regularStage.ARankLocation.HasValue
              && Session!.Locations.AllLocationsChecked.Contains(regularStage.ARankLocation.Value)
            )
            {
              Plugin.Logger.LogInfo("Found A rank location");
              Persistence.SetLevelRank(level, Rank.A, false, false);
            }
            else if (
              regularStage.BRankLocation.HasValue
              && Session!.Locations.AllLocationsChecked.Contains(regularStage.BRankLocation.Value)
            )
            {
              Plugin.Logger.LogInfo("Found B rank location");
              Persistence.SetLevelRank(level, Rank.B, false, false);
            }
            else
            {
              // We haven't cleared the level yet.
              Plugin.Logger.LogInfo("Couldn't find any location");
              Persistence.SetLevelRank(level, Rank.NotFinished, false, false);
              _itemsProcessed--;
              return;
            }
          }
          else if (levelStage is BossStage bossStage)
          {
            if (Session!.Locations.AllLocationsChecked.Contains(bossStage.PerfectLocation))
            {
              Plugin.Logger.LogInfo("Found Perfect rank location");
              Persistence.SetLevelRank(level, Rank.BossPerfect, false, false);
            }
            else if (
              bossStage.CompletePlusLocation.HasValue
              && Session!.Locations.AllLocationsChecked.Contains(bossStage.CompletePlusLocation.Value)
            )
            {
              Plugin.Logger.LogInfo("Found Complete+ rank location");
              Persistence.SetLevelRank(level, Rank.BossNoCheckpoints, false, false);
            }
            else if (Session!.Locations.AllLocationsChecked.Contains(bossStage.ClearLocation))
            {
              Plugin.Logger.LogInfo("Found Clear rank location");
              Persistence.SetLevelRank(level, Rank.BossClear, false, false);
            }
            else
            {
              // We haven't cleared the level yet.
              Plugin.Logger.LogInfo("Couldn't find any location");
              Persistence.SetLevelRank(level, Rank.NotFinished, false, false);
              _itemsProcessed--;
              return;
            }
          }

          // As we've cleared the level, we need to check if we have unlocked a boss song,
          //  and handle its rank if we have unlocked one.
          Plugin.Logger.LogInfo("Checking if boss song unlocked");
          Act act = Bindings.LevelToAct[level];
          if (UnlockItemPatch.HasUnlockedBossSong(act))
          {
            Level[] bossLevels = Bindings.ActBoss[act];

            if (act == Act.Act7)
            {
              scnLevelSelect.instance?.UnlockEntrance(Region.RecordsRoom); // elevator
            }

            foreach (Level bossLevel in bossLevels)
            {
              Plugin.Logger.LogInfo($"Attempting to get boss rank from locations cleared for {bossLevels}");
              BossStage bossStage = (BossStage)Bindings.LevelToStage[bossLevel];
              if (Session!.Locations.AllLocationsChecked.Contains(bossStage.PerfectLocation))
              {
                Plugin.Logger.LogInfo("Found Perfect location");
                Persistence.SetLevelRank(bossLevel, Rank.BossPerfect, false, false);
              }
              else if (
                bossStage.CompletePlusLocation.HasValue
                && Session!.Locations.AllLocationsChecked.Contains(bossStage.CompletePlusLocation.Value)
              )
              {
                Plugin.Logger.LogInfo("Found Complete+ location");
                Persistence.SetLevelRank(bossLevel, Rank.BossNoCheckpoints, false, false);
              }
              else if (Session!.Locations.AllLocationsChecked.Contains(bossStage.ClearLocation))
              {
                Plugin.Logger.LogInfo("Found Clear location");
                Persistence.SetLevelRank(bossLevel, Rank.BossClear, false, false);
              }
              else
              {
                // We haven't cleared the boss level yet.
                Plugin.Logger.LogInfo("Couldn't find any location");
                Persistence.SetLevelRank(bossLevel, Rank.NotFinished, false, false);
              }
            }
          }
        }
      }
      else
      {
        Persistence.SetLevelRank(level, Rank.NotFinished, false, false);
      }
      _itemsProcessed--;
      return;
    }
    if (Bindings.TrapItemIdToLevel.TryGetValue(item.ItemId, out Type trap))
    {
      Plugin.Logger.LogInfo($"Adding trap item {item.ItemName} ({item.ItemId})");
      await TrapManager.AddTrap(trap);
      _itemsProcessed--;
      return;
    }
    if (Bindings.KeyItemIdToWard.TryGetValue(item.ItemId, out Region region))
    {
      // We also do this in UnlockItemPatch,
      // but regions must also be able to be unlocked while in level select.
      if (scnBase.instance is scnLevelSelect)
      {
        Plugin.Logger.LogInfo($"Unlocking entrance {region}");
        scnLevelSelect.instance.UnlockEntrance(region);
      }
      else
      {
        Plugin.Logger.LogInfo("Got region key, but not in level select so ignoring");
        _itemsProcessed--;
        return;
      }
    }
    else if (Bindings.SLEEVE_PAINT_ITEM_ID == item.ItemId)
    {
      Harmony.UnpatchID(Plugin.PATCH_ID_SLEEVE_PAINT);

      // Unity will crash if we do not call this on the main thread.
      Plugin.Logger.LogDebug("Queueing reloading Sleeve Paint on main thread");
      Plugin.ToExecuteOnMainThread.Enqueue(() =>
      {
        // Reload our actual Sleeve Paint.
        Plugin.Logger.LogInfo($"[{nameof(Client)}] Reloading Sleeve Paint");
        Persistence.p1Skin.Reload();
        Persistence.p2Skin.Reload();
      });

      _itemsProcessed--;
      return;
    }
    // ReSharper restore NullableWarningSuppressionIsUsed

    // TODO: implement else case for filler like A Bit of Rhythm
    Plugin.Logger.LogError($"Got item {item.ItemName} ({item.ItemId} from {item.ItemGame}) but couldn't handle it");
    _itemsProcessed--;
  }

  private void DeathLinkReceived(DeathLink deathLink)
  {
    Plugin.Logger.LogInfo($"DeathLink from {deathLink.Source} by \"{deathLink.Cause}\" at {deathLink.Timestamp}");

    if (!DeathLinkPatch.enabled)
      return;

    string text = string.IsNullOrWhiteSpace(deathLink.Cause) ? $"{deathLink.Source} died" : deathLink.Cause;

    if (scnGame.instance is not null)
    {
      if (scnGame.instance.levelIdentifier == nameof(Level.BeansHopper))
      {
        Plugin.Logger.LogInfo("Running tag 'miss'");

        // While Beans Hopper does technically have hearts, they're not visible/relevant in this minigame.
        DeathLinkPatch.enabled = false;
        scnGame.instance.currentLevel.RunTag("miss");

        // FIXME: Doesn't show - gets overwritten by score text.
        //scnGame.instance.statusText.SetStatusText(text, Color.red, narrate: true);
      }
      else
      {
        // Normal/boss level.
        Plugin.Logger.LogInfo("Breaking all hearts");

        scrConductor.PlayFeedback(GameSoundType.BigMistake, group: RDUtils.GetMixerGroup("MistakesParent"));
        scnGame.instance.FlashBorderFeedbackWithDuration(scnGame.BorderFeedbackType.Incorrect, 5f);
        scnGame.instance.ShakeAllHearts(duration: 1f, 8);
        scnGame.instance.ShakeAllCharacters(duration: 1f, 8);
        scnGame.instance.currentLevel.CrackAllHearts();
        scnGame.instance.mistakesManager.mistakesCountP1 += 500;
        scnGame.instance.mistakesManager.mistakesCountP2 += 500;
        DeathLinkPatch.enabled = false;

        if (scnGame.instance.currentLevel.shouldMakeHealthBar)
        {
          // TODO: If possible, use the last entity interacted with (missed/hit note)

          scnGame.instance.UpdatePlayerHealthBars();
          if (!scnGame.instance.currentLevel.noBossFail)
          {
            scnGame.instance.FailLevel(scnGame.instance.rows[0].ent);
          }
        }

        // We only show the status text after we (potentially game over) as it can overwrite its text
        scnGame.instance.statusText.SetStatusText(text, Color.red, 10f, true, true);
      }
    }
    else if (scnBase.instance is scnIanDesktop desktop)
    {
      if (desktop.state != scnIanDesktop.ComputerState.Desktop)
        return;

      switch (desktop.currentProgramIndex)
      {
        // Rhythm Stacker
        case 0:
          Plugin.Logger.LogInfo("Killing stacker");
          // from AddBlock()
          desktop.stackerManager.gameoverContainer.SetActive(true);
          desktop.stackerManager.hasLost = true;
          desktop.stackerManager.hiScoreText.text = RDString
            .Get("rhythmStacker.hiScore")
            .Replace("[score]", desktop.stackerManager.highestScore.ToString(), StringComparison.Ordinal);
          RDStringToUIText.Apply(desktop.stackerManager.hiScoreText);
          // TODO: Use game over text instead of high score text
          //  - this will require a patch to reset the text after Restart().
          desktop.stackerManager.hiScoreText.text = text + "\n" + desktop.stackerManager.hiScoreText.text;
          // TODO: Maybe sync high score with DataStorage?
          desktop.stackerManager.PlaySound("sndDesktopJingleNeutral");
          break;
        // tempres
        case 1:
          Plugin.Logger.LogInfo("Killing tempres");
          // TODO: Show person who killed them
          // FIXME: Technically this works but its a bit buggy, doesn't tween bars.
          //  Also doesn't account for login minigame.
          // Use a reverse transpiler to pull out the `if (freeplay)` block and invoke it here.
          desktop.tempresManager.currentGameHasFinished = true;
          break;
      }
    }
  }

  #region Replicate state
  private void ReplicatePaigeStays(JToken oldValue, JToken newValue, Dictionary<string, JToken> _)
  {
    Plugin.Logger.LogInfo($"[Replication] Paige stays {oldValue}->{newValue}");
    Persistence.SetPaigeEnding(newValue.ToObject<bool>());
  }

  private void ReplicateIansDesktopUnlocked(JToken oldValue, JToken newValue, Dictionary<string, JToken> _)
  {
    Plugin.Logger.LogInfo($"[Replication] Ian's desktop unlocked {oldValue}->{newValue}");
    Persistence.SetIanDesktopLogin(newValue.ToObject<bool>());
  }
  #endregion

  internal void SendDeathLink()
  {
    // ReSharper disable once NullableWarningSuppressionIsUsed
    PlayerInfo player = Session!.Players.ActivePlayer;

    string message = player.Alias + DeathLinkMessages[Plugin.Random.Next(DeathLinkMessages.Length)];

    DeathLink deathLink = new(player.Alias, message);
    SendDeathLink(deathLink);
  }

  private void SendDeathLink(DeathLink deathLink)
  {
    Plugin.Client.DeathLink?.SendDeathLink(deathLink);
    Plugin.Logger.LogInfo($"Sent death link: \"{deathLink.Cause}\"");
  }

  public void Dispose()
  {
    IEnumerator DisconnectSession(ArchipelagoSession __session)
    {
      Plugin.Logger.LogInfo("Disconnecting session...");
      Task disconnect = Task.Run(__session.Socket.DisconnectAsync);
      yield return new WaitUntil(() => disconnect.IsCompleted);
    }

    _cancellationTokenSource.Cancel();
    _cancellationTokenSource.Dispose();

    if (Session is not null)
    {
      Plugin.Instance.StartCoroutine(DisconnectSession(Session));
    }
    TrapManager.Dispose();
  }
}
