namespace RhythmDoctor.Archipelago.Client;

/// <summary>
/// Archipelago client
/// </summary>
internal sealed class Client
{
  internal ArchipelagoSession? session;
  internal DeathLinkService? deathLinkService;

  internal SlotData slotData;
  internal TrapManager trapManager;

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
        foreach (ReceivedItemsHelper item in itemQueue)
        {
          ProcessItem(item, true);
        }
      }
    }
  }

  /// <summary>
  /// Item queue for items received while we were not <see cref="ReadyForItems"/>.
  /// </summary>
  /// <seealso cref="ReadyForItems"/>
  private Queue<ReceivedItemsHelper> itemQueue;
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
    itemQueue = new();
    trapManager = new();

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
  }

#if DEBUG
  public Client()
  {
    Plugin.Logger.LogWarning("Creating client with no login");

    itemQueue = new();
    trapManager = new();
  }
#endif

  /// <summary>
  /// Create an Archipelago session.
  /// </summary>
  /// <param name="server">Server to connect to</param>
  /// <returns>Created session</returns>
  internal ArchipelagoSession CreateSession(string server)
  {
    Plugin.Logger.LogInfo($"Creating Archipelago session to {server}");
    session = ArchipelagoSessionFactory.CreateSession(server);
    return session;
  }

  /// <summary>
  /// Connect to an Archipelago room.
  /// </summary>
  /// <param name="name">Slot name to connect under</param>
  /// <param name="password">Password of the room</param>
  /// <param name="deathLink">Whether to enable DeathLink</param>
  /// <exception cref="NullReferenceException">Session is null</exception>
  /// <exception cref="Exception">Login failure</exception>
  internal void Connect(string name, string? password = null, bool deathLink = false)
  {
    if (session == null)
    {
      throw new NullReferenceException("Session is null");
    }

    if (deathLink)
    {
      deathLinkService = session.CreateDeathLinkService();
      deathLinkService.EnableDeathLink();
      deathLinkService.OnDeathLinkReceived += DeathLinkReceived;
    }

    LoginResult loginResult = session.TryConnectAndLogin(
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
      Plugin.Logger.LogError(loginFailure.Errors);
      Plugin.Logger.LogError(loginFailure.ErrorCodes);

      // FIXME: Shouldn't use a generic exception for this
      throw new Exception($"Failed to connect to server under {name}: {password} (Death Link: {deathLink})");
    }
    else if (loginResult is LoginSuccessful loginSuccessful)
    {
      Plugin.Logger.LogInfo(
        $"Successfully connected to {loginSuccessful.Slot}/{name} as {session.ConnectionInfo.Uuid}"
      );

      Plugin.Logger.LogDebug("Binding events");
      session.MessageLog.OnMessageReceived += MessageReceived;
      session.Items.ItemReceived += ItemReceived;

      slotData = new SlotData(loginSuccessful.SlotData);
    }
    else
    {
      // FIXME: Shouldn't use a generic exception?
      throw new Exception($"Unknown error: failed to connect to {name}");
    }
  }

  internal void MessageReceived(LogMessage message)
  {
    Plugin.Logger.LogInfo($"Received message {message}");
  }

  internal void ItemReceived(ReceivedItemsHelper helper) => ProcessItem(helper);

  internal void ProcessItem(ReceivedItemsHelper helper, bool wasQueued = false)
  {
    ItemInfo item;

    // Ensure the save is prepared before we attempt to load our existing items.
    if (!ReadyForItems)
    {
      item = helper.PeekItem();
      Plugin.Logger.LogInfo($"Enqueued item {item.ItemName} ({item.ItemId} from {item.ItemGame})");
      itemQueue.Enqueue(helper);
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
    if (Bindings.StageItemIdToLevel.TryGetValue(item.ItemId, out Level level))
    {
      Plugin.Logger.LogInfo($"Unlocking stage item {item.ItemName} ({item.ItemId}, {level})");

      if (wasQueued)
      {
        Plugin.Logger.LogInfo($"Attempting to get rank from locations cleared for {level}");

        // Attempt to get rank from locations sent
        RegularStage levelStage = (RegularStage)Bindings.LevelToStage[level];
        if (session!.Locations.AllLocationsChecked.Contains(levelStage.SRankLocation))
        {
          Plugin.Logger.LogInfo("Found S rank location");
          Persistence.SetLevelRank(level, Rank.S, false, false);
        }
        else if (
          levelStage.ARankLocation.HasValue
          && session!.Locations.AllLocationsChecked.Contains(levelStage.ARankLocation.Value)
        )
        {
          Plugin.Logger.LogInfo("Found A rank location");
          Persistence.SetLevelRank(level, Rank.A, false, false);
        }
        else if (
          levelStage.BRankLocation.HasValue
          && session!.Locations.AllLocationsChecked.Contains(levelStage.BRankLocation.Value)
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

        // As we've cleared the level, we need to check if we have unlocked a boss song,
        //  and handle its rank if we have unlocked one.
        Plugin.Logger.LogInfo("Checking if boss song unlocked");
        Act act = Bindings.LevelToAct[level];
        Level bossLevel = Bindings.ActBoss[act];
        if (UnlockItemPatch.HasUnlockedBossSong(act))
        {
          Plugin.Logger.LogInfo($"Attempting to get boss rank from locations cleared for {bossLevel}");
          BossStage bossStage = (BossStage)Bindings.LevelToStage[bossLevel];
          if (session!.Locations.AllLocationsChecked.Contains(bossStage.PerfectLocation))
          {
            Plugin.Logger.LogInfo("Found Perfect location");
            Persistence.SetLevelRank(bossLevel, Rank.BossPerfect, false, false);
          }
          else if (
            bossStage.CompletePlusLocation.HasValue
            && session!.Locations.AllLocationsChecked.Contains(bossStage.CompletePlusLocation.Value)
          )
          {
            Plugin.Logger.LogInfo("Found Complete+ location");
            Persistence.SetLevelRank(bossLevel, Rank.BossNoCheckpoints, false, false);
          }
          else if (session!.Locations.AllLocationsChecked.Contains(bossStage.ClearLocation))
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
      else
      {
        Persistence.SetLevelRank(level, Rank.NotFinished, false, false);
      }
      return;
    }
    else if (Bindings.TrapItemIdToLevel.TryGetValue(item.ItemId, out Type trap))
    {
      Plugin.Logger.LogInfo($"Adding trap item {item.ItemName} ({item.ItemId})");
      trapManager.AddTrap(trap);
      return;
    }
    else if (Bindings.KeyItemIdToWard.ContainsKey(item.ItemId))
    {
      // We do this in UnlockItemPatch
      Plugin.Logger.LogInfo($"Ignoring key item {item.ItemName} ({item.ItemId})");
      return;
    }
    // ReSharper restore NullableWarningSuppressionIsUsed

    // TODO: implement else case for filler like A Bit of Rhythm
    Plugin.Logger.LogError($"Got item {item.ItemName} ({item.ItemId} from {item.ItemGame}) but couldn't handle it");
  }

  internal void DeathLinkReceived(DeathLink deathLink)
  {
    Plugin.Logger.LogInfo($"DeathLink from {deathLink.Source} by \"{deathLink.Cause}\" at {deathLink.Timestamp}");
    // TODO: Implement
  }
}
