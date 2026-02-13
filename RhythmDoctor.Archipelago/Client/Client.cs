using Archipelago.MultiClient.Net.Packets;
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
  internal TrapManager TrapManager;

  #region Ready for items
  /// <summary>
  /// Whether to process items or not.
  /// Items received when this is false will be put in <see cref="itemQueue"/>
  /// </summary>
  /// <seealso cref="itemQueue"/>
  private bool ReadyForItems
  {
    get;
    set
    {
      field = value;
      if (value)
      {
        Plugin.Logger.LogInfo("Processing all queued items");
        foreach (ReceivedItemsHelper item in itemQueue!)
        {
          ProcessItem(item, true);
        }
        itemQueue = null;
      }
    }
  }

  /// <summary>
  /// Item queue for items received while we were not <see cref="ReadyForItems"/>.
  /// </summary>
  private Queue<ReceivedItemsHelper>? itemQueue;
  #endregion

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
    itemQueue = new Queue<ReceivedItemsHelper>();
    TrapManager = new TrapManager();
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

    Plugin.Logger.LogDebug("Binding events");
    Session.MessageLog.OnMessageReceived += MessageReceived;
    Session.Items.ItemReceived += ItemReceived;
    Session.DataStorage[Scope.Slot, Persistence.PaigeStaysKey].OnValueChanged += ReplicatePaigeStays;

    return Session;
  }

  /// <summary>
  /// Connect to an Archipelago room.
  /// </summary>
  /// <remarks>
  /// This method does not load the Level Select.
  /// </remarks>
  /// <param name="name">Slot name to connect under</param>
  /// <param name="password">Password of the room</param>
  /// <exception cref="NullReferenceException">Session is null</exception>
  /// <exception cref="Exception">Login failure</exception>
  private async Task<LoginResult> Connect(string name, string? password = null)
  {
    if (Session == null)
    {
      throw new NullReferenceException("Session is null");
    }

    Plugin.Logger.LogInfo("Attempting to connect");
    RoomInfoPacket _ = await Session.ConnectAsync();
    Plugin.Logger.LogInfo("Connected");

    Plugin.Logger.LogInfo("Attempting to login");
    LoginResult loginResult = await Session.LoginAsync(
      "Rhythm Doctor",
      name,
      ItemsHandlingFlags.AllItems,
      new Version("0.6.3"),
      null, // DeathLink is managed by DeathLinkService
      null, // Randomly generated
      password
    );
    Plugin.Logger.LogInfo("Logged in");

    switch (loginResult)
    {
      case LoginFailure loginFailure:
      {
        Plugin.Logger.LogError("Login failed");
        for (int i = 0; i < loginFailure.Errors.Length; i++)
        {
          string error = loginFailure.Errors[i];
          ConnectionRefusedError errorCode = loginFailure.ErrorCodes[i];

          Plugin.Logger.LogError($"Error {errorCode}: {error}");
        }

        return loginResult;
      }
      case LoginSuccessful loginSuccessful:
      {
        Plugin.Logger.LogInfo(
          $"Successfully logged into {loginSuccessful.Slot}/{name} as {Session.ConnectionInfo.Uuid}"
        );

        Configuration.DeathLinkConfig deathLink = await Configuration.GetDeathLink();
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

        return loginResult;
      }
      default:
        throw new Exception($"Unknown error: failed to connect to {name}");
    }
  }

  private void PrepareSlot()
  {
    if (Session == null)
    {
      throw new NullReferenceException("Session is null");
    }
    // TODO: Check if session exists but is invalid

    // Setup DataStorage and TrapManager.ClearedTraps, Sticky Traps, initial Paige stays state (this can change!)
    Plugin
      .Client.Session!.DataStorage[Scope.Slot, Persistence.PaigeStaysKey]
      .Initialize(Plugin.Random.Next() % 2 == 1);
    Persistence.SetPaigeEnding(Plugin.Client.Session!.DataStorage[Scope.Slot, Persistence.PaigeStaysKey].To<bool>());
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
    LoginResult loginResult = await Connect(name, password);
    if (!loginResult.Successful)
      return loginResult;

    PrepareSlot();
    ReadyForItems = true;

    return loginResult;
  }

  private void MessageReceived(LogMessage message)
  {
    Plugin.Logger.LogInfo($"Received message \"{message}\"");
  }

  private void ItemReceived(ReceivedItemsHelper helper) => ProcessItem(helper);

  private void ProcessItem(ReceivedItemsHelper helper, bool wasQueued = false)
  {
    ItemInfo item;

    // Ensure the save is prepared before we attempt to load our existing items.
    if (!ReadyForItems)
    {
      item = helper.PeekItem();
      Plugin.Logger.LogInfo($"Enqueued item {item.ItemName} ({item.ItemId} from {item.ItemGame})");
      itemQueue!.Enqueue(helper);
      return;
    }

    item = helper.DequeueItem();
    if (item.ItemGame != Bindings.GAME)
    {
      Plugin.Logger.LogDebug(
        $"Ignoring item {item.ItemName} ({item.ItemId} from {item.ItemGame}), as it is not for our world"
      );
      return;
    }

    // ReSharper disable NullableWarningSuppressionIsUsed
    if (Bindings.ItemIdToLevel.TryGetValue(item.ItemId, out Level level))
    {
      Plugin.Logger.LogInfo($"[{nameof(Client)}] Unlocking stage item {item.ItemName} ({item.ItemId}, {level})");

      if (wasQueued)
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
              scnLevelSelect.instance.UnlockAbandonedWard();
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
      return;
    }
    else if (Bindings.TrapItemIdToLevel.TryGetValue(item.ItemId, out Type trap))
    {
      Plugin.Logger.LogInfo($"Adding trap item {item.ItemName} ({item.ItemId})");
      TrapManager.AddTrap(trap);
      return;
    }
    else if (Bindings.KeyItemIdToWard.TryGetValue(item.ItemId, out Region region))
    {
      // We also do this in UnlockItemPatch,
      // but regions are able to be unlocked cleanly while in level select.
      if (scnBase.instance is scnLevelSelect)
      {
        Plugin.Logger.LogInfo($"Unlocking entrance {region}");
        scnLevelSelect.instance.UnlockEntrance(region);
      }
      else
      {
        Plugin.Logger.LogInfo("Got region key, but not in level select so ignoring");
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
        Persistence.p1Skin.Reload();
        Persistence.p2Skin.Reload();
      });

      return;
    }
    // ReSharper restore NullableWarningSuppressionIsUsed

    // TODO: implement else case for filler like A Bit of Rhythm
    Plugin.Logger.LogError($"Got item {item.ItemName} ({item.ItemId} from {item.ItemGame}) but couldn't handle it");
  }

  private void DeathLinkReceived(DeathLink deathLink)
  {
    Plugin.Logger.LogInfo($"DeathLink from {deathLink.Source} by \"{deathLink.Cause}\" at {deathLink.Timestamp}");

    if (!DeathLinkPatch.enabled)
      return;

    string text = string.IsNullOrWhiteSpace(deathLink.Cause) ? $"{deathLink.Source} died" : deathLink.Cause;

    if (scnGame.instance is not null)
    {
      if (scnGame.instance.levelIdentifier == "BeansHopper")
      {
        // While Beans Hopper does technically have hearts, they're not visible/relevant in this minigame.
        scnGame.instance.currentLevel.RunTag("miss");

        // FIXME: Doesn't show - gets overwritten by score text.
        //scnGame.instance.statusText.SetStatusText(text, Color.red, narrate: true);
      }
      else
      {
        // Normal/boss level.
        Plugin.Logger.LogInfo("Cracking all hearts");

        scrConductor.PlayFeedback(GameSoundType.BigMistake, group: RDUtils.GetMixerGroup("MistakesParent"));
        scnGame.instance.FlashBorderFeedbackWithDuration(scnGame.BorderFeedbackType.Incorrect, 5f);
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
            .Replace("[score]", desktop.stackerManager.highestScore.ToString());
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
  #endregion

  internal void SendDeathLink()
  {
    // ReSharper disable once NullableWarningSuppressionIsUsed
    PlayerInfo player = Session!.Players.ActivePlayer;

    string message = player.Alias + DeathLinkMessages[Plugin.Random.Next(DeathLinkMessages.Length)];

    DeathLink deathLink = new(player.Alias, message);
    SendDeathLink(deathLink);
  }

  internal void SendDeathLink(DeathLink deathLink)
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

    if (Session != null)
    {
      Session.MessageLog.OnMessageReceived -= MessageReceived;
      Session.Items.ItemReceived -= ItemReceived;
      Session.DataStorage[Scope.Slot, Persistence.PaigeStaysKey].OnValueChanged -= ReplicatePaigeStays;

      if (DeathLink != null)
      {
        DeathLink.OnDeathLinkReceived -= DeathLinkReceived;
      }
      Plugin.Instance.StartCoroutine(DisconnectSession(Session));
    }
    TrapManager.Dispose();
  }
}
