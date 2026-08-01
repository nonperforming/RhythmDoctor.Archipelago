using Archipelago.MultiClient.Net.Packets;

namespace RhythmDoctor.Archipelago.Client;

/// <summary>
/// Archipelago client for the story mode.
/// </summary>
internal sealed class StoryClient : IDisposable, IAsyncDisposable
{
  // Login information
  internal LoginInformation LoginInformation { get; private set; }

  // Components
  internal ItemProcessorClientComponent[] ItemProcessorComponents { get; private set; } =
  [new StoryLevelItemProcessorClientComponent(), new TrapItemProcessorClientComponent()];

  internal ArchipelagoModifierManagerClientComponent? ModifierManagerComponent { get; private set; } = new();
  internal DeathLinkClientComponent? DeathLinkComponent { get; private set; }
  internal ReplicationClientComponent? ReplicationComponent { get; private set; }

  // State
  internal ClientState State { get; private set; } = ClientState.NotReady;
  internal SlotData Slot { get; private set; }

  /// <summary>
  /// Patches that are applied after logging into Archipelago, and unapplied after logging out.
  /// </summary>
  private static readonly Type[] PostLoginPatches =
  [
    typeof(Act5Patch),
    typeof(ClearStoryLocationPatch),
    typeof(DeathLinkPatch),
    //typeof(JanitorPatch), // use pause menu for in/outbox
    typeof(LevelSelectVisualFixesPatch),
    typeof(RhythmDogtorLevelPatch),
    typeof(RhythmWeightlifterPatch),
    typeof(RunningCharactersPatch),
    typeof(SkipCutscenePatch),
    typeof(SkipTutorialPatch),
    typeof(UnlockItemPatch),
    typeof(WelcomeBackPatch),
    typeof(SavingPatch),
    typeof(UnapplyPatchesPatch),
  ];

  internal IEnumerable<IClientComponent> ClientComponents
  {
    get
    {
      foreach (ItemProcessorClientComponent itemProcessorComponent in ItemProcessorComponents)
        yield return itemProcessorComponent;
      if (ModifierManagerComponent is not null)
        yield return ModifierManagerComponent;
      if (DeathLinkComponent is not null)
        yield return DeathLinkComponent;
      if (ReplicationComponent is not null)
        yield return ReplicationComponent;
    }
  }

  internal ArchipelagoSession? Session { get; private set; }

  private readonly CancellationTokenSource _cancellationTokenSource = new();

  internal StoryClient(LoginInformation loginInformation)
  {
    LoginInformation = loginInformation;
  }

  internal ArchipelagoSession CreateSession()
  {
    void BindEvents(ArchipelagoSession session)
    {
      session.Socket.ErrorReceived += (
        (__exception, __message) =>
          Plugin.Logger.LogError($"[{nameof(StoryClient)}] Socket error {__exception} - {__message}")
      );
      // TODO: attempt reconnect on socket close.
      //       Remember to process items we may have received from time of disconnection to reconnection
      session.Socket.SocketClosed += (
        __reason =>
        {
          Plugin.Logger.LogFatal($"[{nameof(StoryClient)}] Archipelago client closed ({__reason}), disposing...");
          Dispose();
        }
      );
      session.MessageLog.OnMessageReceived += (
        __message => Plugin.Logger.LogInfo($"[{nameof(StoryClient)}] Received message \"{__message}\"")
      );
    }

    ThrowIfNotReadyFor(ClientState.CreatingSession);
    State = ClientState.CreatingSession;
    Plugin.Logger.LogInfo($"[{nameof(StoryClient)}] Creating Archipelago session to {LoginInformation.Uri}");

    Session = ArchipelagoSessionFactory.CreateSession(LoginInformation.Uri);
    BindEvents(Session);
    State = ClientState.CreatedSession;

    return Session;
  }

  /// <summary>
  /// Log into the Archipelago server with the <see cref="LoginInformation"/>
  /// given via <see cref="StoryClient"/>'s constructor.
  /// </summary>
  /// <remarks>
  /// <see cref="CreateSession"/> should be called prior to calling this.
  /// </remarks>
  /// <returns></returns>
  internal async Task<LoginResult> Login()
  {
    // TODO: break into multiple methods, don't login and go to level select here
    ThrowIfNotReadyFor(ClientState.LoggingIn);

    // At this point Session is guaranteed to not be null

    State = ClientState.LoggingIn;
    Plugin.Logger.LogInfo($"[{nameof(StoryClient)}] Connecting...");
    // ReSharper disable once NullableWarningSuppressionIsUsed
    RoomInfoPacket roomInfo = await Session!.ConnectAsync();
    Plugin.Logger.LogInfo($"[{nameof(StoryClient)}] Logging in...");
    // ReSharper disable once NullableWarningSuppressionIsUsed
    LoginResult loginResult = await Session!.LoginAsync(
      Bindings.GAME,
      LoginInformation.SlotName,
      ItemsHandlingFlags.AllItems,
      new Version(0, 6, 7),
      password: LoginInformation.Password
    );

    if (loginResult is not LoginSuccessful loginSuccessful)
    {
      // TODO: handle failure gracefully
      return loginResult as LoginFailure
        ?? throw new InvalidOperationException("Login not successful but not failure either!?");
    }
    Slot = new SlotData(loginSuccessful.SlotData);

    List<Task> clientComponentsToEnable = new();
    ReplicationComponent = new StoryReplicationClientComponent();
    clientComponentsToEnable.Add(ReplicationComponent.Enable(this, Session));

    // Create DeathLink if applicable
    Configuration.DeathLinkConfig deathLinkConfig = await Configuration.GetDeathLink();
    if (
      deathLinkConfig == Configuration.DeathLinkConfig.On
      || (deathLinkConfig == Configuration.DeathLinkConfig.FollowSlot && Slot.deathLink)
    )
    {
      DeathLinkComponent = new DeathLinkClientComponent();
      clientComponentsToEnable.Add(DeathLinkComponent.Enable(this, Session));
    }

    // Wait for all components to enable.
    ItemProcessorComponents = [new StoryLevelItemProcessorClientComponent(), new TrapItemProcessorClientComponent()];
    ModifierManagerComponent = new ArchipelagoModifierManagerClientComponent();
    foreach (ItemProcessorClientComponent itemProcessorClientComponent in ItemProcessorComponents)
    {
      clientComponentsToEnable.Add(itemProcessorClientComponent.Enable(this, Session));
    }
    clientComponentsToEnable.Add(ModifierManagerComponent.Enable(this, Session));
    Task.WaitAll(clientComponentsToEnable.ToArray());

    // Apply necessary patches.
    Plugin.Logger.LogInfo($"[{nameof(StoryClient)}] Applying gameplay patches");
    // TODO: pull this OUT of plugin
    Plugin.ApplyPatches(Plugin.PATCH_ID_POST_LOGIN, PostLoginPatches);
    Plugin.ApplyPatches(Plugin.PATCH_ID_SLEEVE_PAINT, typeof(LockSleevePaintPatch));

    // If we got here then SavingPatch has been applied, and this should be safe.
    Persistence.slotPrefs[Configuration.GetSlotToUse()].dict.Clear();

    // Let LockSleevePaintPatch set the Sleeve Paint to a default colour
    Persistence.p1Skin.Reload();
    Persistence.p2Skin.Reload();

    // Some levels come unlocked by default, such as X-1.
    // Lock all levels to force the user to unlock them with an item.
    foreach (Level level in Enum.GetValues(typeof(Level)))
    {
      Persistence.SetLevelRank(level, Rank.NotAvailable, true);
    }

    State = ClientState.LoggedIn;
    return loginResult;
  }

  internal Task ReceivePriorItems()
  {
    ThrowIfNotReadyFor(ClientState.ReceivingPriorItems);

    // At this point Session is guaranteed to not be null.

    // Process all previously received items...
    Plugin.Logger.LogInfo($"[{nameof(StoryClient)}] Receiving items...");
    State = ClientState.ReceivingPriorItems;

    // ReSharper disable once NullableWarningSuppressionIsUsed
    while (Session!.Items.Any())
    {
      // TODO: async
      HandleInitialItem(Session.Items.DequeueItem());
    }

    UnlockItemPatch.TryUnlockAllBossSongs(true);
    State = ClientState.Ready;
    return Task.CompletedTask;
  }

  internal Task StartPlay()
  {
    if (State != ClientState.Ready)
      throw new InvalidOperationException("Not ready to start play");

    scnBase.GoToScene(GC.SceneLevelSelect);
    return Task.CompletedTask;
  }

  /// <remarks>
  /// Run just after loading Level Select.
  /// </remarks>
  internal void ProcessNewReceivedItems()
  {
    if (State != ClientState.Ready)
      throw new InvalidOperationException("Not ready to process new items yet");

    while (Session!.Items.Any())
    {
      ItemInfo itemInfo = Session!.Items.DequeueItem();
      HandleItem(itemInfo);
    }
  }

  private void HandleInitialItem(ItemInfo itemInfo)
  {
    Plugin.Logger.LogDebug(
      $"[{nameof(StoryClient)}] Processing item initially {itemInfo.ItemName} ({itemInfo.ItemId})"
    );
    foreach (ItemProcessorClientComponent itemProcessorClientComponent in ItemProcessorComponents)
    {
      if (itemProcessorClientComponent.HandleItemInitial(itemInfo))
        break;
    }
  }

  private void HandleItem(ItemInfo itemInfo)
  {
    Plugin.Logger.LogDebug($"[{nameof(StoryClient)}] Processing item {itemInfo.ItemName} ({itemInfo.ItemId})");
    foreach (ItemProcessorClientComponent itemProcessorClientComponent in ItemProcessorComponents)
    {
      if (itemProcessorClientComponent.HandleItem(itemInfo))
        return;
    }
    Plugin.Logger.LogWarning(
      $"[{nameof(StoryClient)}] Item '{itemInfo.ItemName}' (ID: {itemInfo.ItemId}) went through all processors without being processed!"
    );
  }

  private void ThrowIfNotReadyFor(ClientState? wantToGoToState = null)
  {
    if (State == ClientState.Disposed || wantToGoToState == ClientState.Failed)
      throw new InvalidOperationException(
        "Cannot perform any operations on a client that has been disposed or is failed."
      );

    if (wantToGoToState is null)
      return;

    switch (wantToGoToState)
    {
      case ClientState.NotReady:
      case ClientState.CreatingSession:
        // no requirements, default state
        break;
      case ClientState.LoggingIn:
        if (Session is null)
        {
          Plugin.Logger.LogError($"[{nameof(StoryClient)}] Cannot log in without creating session beforehand");
          throw new InvalidOperationException("Cannot log in without creating session beforehand");
        }
        break;
      case ClientState.LoggedIn:
        break;
      case ClientState.ReceivingPriorItems:
        if (Session is null)
        {
          Plugin.Logger.LogError($"[{nameof(StoryClient)}] Cannot recieve prior items in without a session");
          throw new InvalidOperationException("Cannot receieve prior items without a session");
        }
        if (State != ClientState.LoggedIn)
        {
          Plugin.Logger.LogError($"[{nameof(StoryClient)}] Cannot recieve prior items in while logged out");
          throw new InvalidOperationException("Cannot receieve prior items while logged out");
        }
        break;
      default:
        Plugin.Logger.LogError($"[{nameof(StoryClient)}] !?? wantToGoToState {wantToGoToState}");
        throw new ArgumentOutOfRangeException(nameof(wantToGoToState), wantToGoToState, null);
    }
  }

  public void Dispose()
  {
    if (State == ClientState.Disposed)
    {
      Plugin.Logger.LogWarning($"[{nameof(StoryClient)}] Attempted to dispose already disposed client");
      return;
    }

    if (Session?.Socket is not null && Session.Socket.Connected)
    {
      Plugin.Logger.LogInfo($"[{nameof(StoryClient)}] Disconnecting from Archipelago...");
      Task.Run(Session.Socket.DisconnectAsync);
    }

    // Don't return to main menu if we haven't applied any patches yet
    if (
      State
      is ClientState.NotReady
        or ClientState.CreatingSession
        or ClientState.CreatedSession
        or ClientState.LoggingIn
    )
    {
      State = ClientState.Disposed;
      return;
    }

    State = ClientState.Disposed;

    // UnapplyPatchesPatch will unapply patches for us
    scnBase.GoToMainMenu();
  }

  public ValueTask DisposeAsync()
  {
    Dispose();
    return new ValueTask(Task.CompletedTask);
  }
}
