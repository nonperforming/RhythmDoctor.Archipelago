namespace RhythmDoctor.Archipelago.Client;

internal sealed class Client
{
  internal ArchipelagoSession? session;
  internal DeathLinkService? deathLinkService;
  internal SlotData? slotData;

  internal Items items;
  internal Locations locations;
  internal Options options;
  internal World.World world;

  public Client(string server, string username, string? password = null, bool deathLink = false)
  {
    items = new();
    locations = new();
    options = new();

    CreateSession(server);
    Connect(username, password, deathLink);
  }

  internal ArchipelagoSession CreateSession(string server)
  {
    Plugin.Logger?.LogInfo($"Creating Archipelago session to {server}");
    session = ArchipelagoSessionFactory.CreateSession(server);
    return session;
  }

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
      deathLinkService.OnDeathLinkReceived += DeathLinkRecieved;
    }

    LoginResult loginResult = session.TryConnectAndLogin(
      "Rhythm Doctor",
      name,
      ItemsHandlingFlags.AllItems,
      new Version("APWorldInformation.Version"), // FIXME: Create APWorld and load this from shared data on disk
      null, // DeathLink is managed by DeathLinkService
      null, // Randomly generated
      password,
      true
    );

    if (loginResult is LoginFailure loginFailure)
    {
      Plugin.Logger?.LogError(loginFailure.Errors);
      Plugin.Logger?.LogError(loginFailure.ErrorCodes);

      // FIXME: Shouldn't use a generic exception for this
      throw new Exception();
    }
    else if (loginResult is LoginSuccessful loginSuccessful)
    {
      Plugin.Logger?.LogInfo(
        $"Successfully connected to {loginSuccessful.Slot}/{name} as {session.ConnectionInfo.Uuid}"
      );

      slotData = new()
      {
        BossUnlockRequirement = (BossUnlockRequirement)loginSuccessful.SlotData["boss_unlock_requirement"],
        EndGoal = (EndGoal)loginSuccessful.SlotData["end_goal"],
      };

      Plugin.Logger.LogDebug("Binding events");
      session.MessageLog.OnMessageReceived += MessageRecieved;
      session.Items.ItemReceived += ItemReceived;
    }
    else
    {
      // FIXME: Shouldn't use a generic exception?
      throw new Exception($"Unknown error: failed to connect to {name}");
    }
  }

  internal void MessageRecieved(LogMessage message)
  {
    Plugin.Logger.LogInfo($"Received message {message}");
  }

  internal void ItemReceived(ReceivedItemsHelper helper)
  {
    ItemInfo item = helper.PeekItem();

    Plugin.Logger?.LogDebug($"Successfully received item {item.ItemName} - {item.ItemId}");
    helper.DequeueItem();
  }

  internal void DeathLinkRecieved(DeathLink deathLink)
  {
    Plugin.Logger.LogInfo($"DeathLink from {deathLink.Source} by {deathLink.Cause} at {deathLink.Timestamp}");
    // TODO: Implement
  }
}
