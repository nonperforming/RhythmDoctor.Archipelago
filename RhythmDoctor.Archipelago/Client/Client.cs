namespace RhythmDoctor.Archipelago.Client;

internal sealed class Client
{
  private ArchipelagoSession? _session;
  private DeathLinkService? _deathLinkService;

  private Items _items;
  private Locations _locations;
  private Options _options;
  private World.World _world;

  Client(string server, string username, string? password = null)
  {
    _items = new();
    _locations = new();
    _options = new();

    CreateSession(server);
  }

  internal ArchipelagoSession CreateSession(string server)
  {
    Plugin.Logger.LogInfo($"Creating Archipelago session to {server}");
    _session = ArchipelagoSessionFactory.CreateSession(server);
    return _session;
  }

  internal void Connect(string name, string? password = null, bool deathLink = false)
  {
    if (_session == null)
    {
      throw new ArgumentNullException("Session is null");
    }

    if (deathLink)
    {
      _deathLinkService = _session.CreateDeathLinkService();
      _deathLinkService.EnableDeathLink();
      _deathLinkService.OnDeathLinkReceived += DeathLinkRecieved;
    }

    LoginResult loginResult = _session.TryConnectAndLogin(
      "Rhythm Doctor",
      name,
      ItemsHandlingFlags.AllItems,
      new Version("APWorldInformation.Version"), // FIXME: Create APWorld and load this from shared data on disk
      null, // DeathLink is managed by DeathLinkService
      null, // Randomly generated
      password,
      true
    );

    if (loginResult.Successful)
    {
      Plugin.Logger.LogInfo($"Successfully connected to {name} as {_session.ConnectionInfo.Uuid}");

      Plugin.Logger.LogDebug("Binding events");
      _session.MessageLog.OnMessageReceived += MessageRecieved;
      _session.Items.ItemReceived += ItemReceived;
    }
    else
    {
      throw new Exception($"Failed to connect to {name}");
    }
  }

  internal void MessageRecieved(LogMessage message)
  {
    Plugin.Logger.LogInfo($"Recieved message {message.ToString()}");
  }

  internal void ItemReceived(ReceivedItemsHelper helper)
  {
    ItemInfo item = helper.PeekItem();

    Plugin.Logger.LogDebug($"Successfully received item {item.ItemName} - {item.ItemId}");
    helper.DequeueItem();
  }

  internal void DeathLinkRecieved(DeathLink deathLink)
  {
    Plugin.Logger.LogInfo($"DeathLink from {deathLink.Source} by {deathLink.Cause} at {deathLink.Timestamp}");
    // TODO: Implement
  }
}
