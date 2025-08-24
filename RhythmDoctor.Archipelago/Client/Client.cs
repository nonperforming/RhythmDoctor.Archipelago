namespace RhythmDoctor.Archipelago.Client;

/// <summary>
/// Archipelago client
/// </summary>
internal sealed class Client
{
  internal ArchipelagoSession? session;
  internal DeathLinkService? deathLinkService;

  // TODO
  //internal SlotData? slotData;

  internal TrapManager trapManager;

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

      // FIXME: InvalidCastException: Specified cast is not valid.
      //slotData = new()
      //{
      //  BossUnlockRequirement = (BossUnlockRequirement)loginSuccessful.SlotData["boss_unlock_requirement"],
      //  EndGoal = (EndGoal)loginSuccessful.SlotData["end_goal"],
      //};

      Plugin.Logger.LogDebug("Binding events");
      session.MessageLog.OnMessageReceived += MessageReceived;
      session.Items.ItemReceived += ItemReceived;

      // TODO: We need to process items already in our inventory!
      //       The player might have gotten items while offline (i.e: playing async)
      //       Iterate through session.Items.AllItemsReceived, and process them accordingly.
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

  internal void ItemReceived(ReceivedItemsHelper helper)
  {
    // TODO: Handle items
    ItemInfo item = helper.DequeueItem();
    if (item.ItemGame != Bindings.GAME)
    {
      Plugin.Logger.LogDebug(
        $"Ignoring item {item.ItemName} ({item.ItemId} from {item.ItemGame}), as it is not for our world"
      );
      return;
    }

    if (Bindings.StageItemIdToLevel.TryGetValue(item.ItemId, out Level level))
    {
      Plugin.Logger.LogInfo($"Unlocking stage item {item.ItemName} ({item.ItemId}, {level})");
      // FIXME: 1-CNY and 1-BOO will not unlock even if this is on - need to patch to force available
      Persistence.SetLevelRank(level, Rank.NotFinished, false, false);
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
    // TODO: implement else case for filler like A Bit of Rhythm
    Plugin.Logger.LogError($"Got item {item.ItemName} ({item.ItemId} from {item.ItemGame}) but couldn't handle it");
  }

  internal void DeathLinkReceived(DeathLink deathLink)
  {
    Plugin.Logger.LogInfo($"DeathLink from {deathLink.Source} by \"{deathLink.Cause}\" at {deathLink.Timestamp}");
    // TODO: Implement
  }
}
