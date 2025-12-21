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
  /// Backing store for <see cref="ReadyForItems"/>.
  /// </summary>
  /// <seealso cref="ReadyForItems"/>
  private bool _readyForItems;

  /// <summary>
  /// Whether to process items or not.
  /// Items received when this is false will be put in <see cref="itemQueue"/>
  /// </summary>
  /// <seealso cref="itemQueue"/>
  internal bool ReadyForItems
  {
    get => _readyForItems;
    set
    {
      _readyForItems = value;
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

  /// <summary>
  /// Create an Archipelago client.
  /// </summary>
  /// <param name="server">The server to connect to</param>
  /// <param name="username">The slot name</param>
  /// <param name="password">The password (if any)</param>
  /// <param name="deathLink">Whether to enable Death Link</param>
  /// <exception cref="Exception">Login failure</exception>
  public Client(string server, string username, string? password = null, bool deathLink = false)
  {
    itemQueue = new Queue<ReceivedItemsHelper>();

    CreateSession(server);
    try
    {
      Connect(username, password, deathLink);
    }
    catch (Exception exception)
    {
      throw new Exception(
        $"Failed to connect to server {server} under {username}: {password} (Death Link: {deathLink})",
        exception
      );
    }

    TrapManager = new TrapManager();
  }

#if DEBUG
  public Client()
  {
    Plugin.Logger.LogWarning("Creating client with no login");

    itemQueue = new Queue<ReceivedItemsHelper>();
    TrapManager = new TrapManager();
  }
#endif

  /// <summary>
  /// Create an Archipelago session.
  /// </summary>
  /// <param name="server">Server to connect to</param>
  /// <returns>Created session</returns>
  private ArchipelagoSession CreateSession(string server)
  {
    Plugin.Logger.LogInfo($"Creating Archipelago session to {server}");
    Session = ArchipelagoSessionFactory.CreateSession(server);
    return Session;
  }

  /// <summary>
  /// Connect to an Archipelago room.
  /// </summary>
  /// <param name="name">Slot name to connect under</param>
  /// <param name="password">Password of the room</param>
  /// <param name="deathLink">Whether to enable DeathLink</param>
  /// <exception cref="NullReferenceException">Session is null</exception>
  /// <exception cref="Exception">Login failure</exception>
  private void Connect(string name, string? password = null, bool deathLink = false)
  {
    if (Session == null)
    {
      throw new NullReferenceException("Session is null");
    }

    Plugin.Logger.LogDebug("Binding events");
    Session.MessageLog.OnMessageReceived += MessageReceived;
    Session.Items.ItemReceived += ItemReceived;

    if (deathLink)
    {
      Plugin.Logger.LogInfo("Creating DeathLink");
      DeathLink = Session.CreateDeathLinkService();
      DeathLink.EnableDeathLink();
      DeathLink.OnDeathLinkReceived += DeathLinkReceived;
    }

    Plugin.Logger.LogDebug("Attempting to login");
    LoginResult loginResult = Session.TryConnectAndLogin(
      "Rhythm Doctor",
      name,
      ItemsHandlingFlags.AllItems,
      new Version("0.6.3"),
      null, // DeathLink is managed by DeathLinkService
      null, // Randomly generated
      password,
      true
    );

    if (loginResult is LoginFailure loginFailure)
    {
      Plugin.Logger.LogError("Login failed");
      for (int i = 0; i < loginFailure.Errors.Length; i++)
      {
        string error = loginFailure.Errors[i];
        ConnectionRefusedError errorCode = loginFailure.ErrorCodes[i];

        Plugin.Logger.LogError($"Error {errorCode}: {error}");
      }

      // FIXME: Shouldn't use a generic exception for this
      throw new Exception($"Failed to connect to server under {name}: {password} (Death Link: {deathLink})");
    }
    else if (loginResult is LoginSuccessful loginSuccessful)
    {
      Plugin.Logger.LogInfo(
        $"Successfully connected to {loginSuccessful.Slot}/{name} as {Session.ConnectionInfo.Uuid}"
      );

      Slot = new SlotData(loginSuccessful.SlotData);
    }
    else
    {
      // FIXME: Shouldn't use a generic exception?
      throw new Exception($"Unknown error: failed to connect to {name}");
    }
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
    // TODO: Implement
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
      if (DeathLink != null)
      {
        DeathLink.OnDeathLinkReceived -= DeathLinkReceived;
      }
      Plugin.Instance.StartCoroutine(DisconnectSession(Session));
    }
    TrapManager.Dispose();
  }
}
