using System.Collections.ObjectModel;

namespace RhythmDoctor.Archipelago.Client;

using Components.ItemProcessors;

using global::Archipelago.MultiClient.Net.Packets;

/// <summary>
/// Archipelago client for the story mode.
/// </summary>
internal sealed class StoryClient : IDisposable, IAsyncDisposable
{
  // Login information
  internal LoginInformation LoginInformation { get; private set; }
  internal SlotData SlotData { get; private set; }

  // Components
  internal ItemProcessorClientComponent[] ItemProcessorComponents { get; private set; } = [new StoryLevelItemProcessorClientComponent(), new TrapItemProcessorClientComponent()];
  //internal ArchipelagoTrapManagerClientComponent? ModifierManagerComponent { get; private set; } =
  //  new();
  internal DeathLinkClientComponent? DeathLinkComponent { get; private set; }
  internal ReplicationClientComponent? ReplicationComponent { get; private set; }

  // State
  internal ClientState State { get; private set; } = ClientState.NotReady;
  internal SlotData Slot { get; private set; }

  internal IEnumerable<ClientComponent> ClientComponents
  {
    get
    {
      foreach (ItemProcessorClientComponent itemProcessorComponent in ItemProcessorComponents)
        yield return itemProcessorComponent;
      //if (ModifierManagerComponent != null)
      //  yield return ModifierManagerComponent;
      if (DeathLinkComponent != null)
        yield return DeathLinkComponent;
      if (ReplicationComponent != null)
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
      session.Socket.SocketClosed += (__reason =>
      {
        Plugin.Logger.LogFatal($"[{nameof(StoryClient)}] Archipelago client closed ({__reason}), returning to Main Menu...");
        scnBase.GoToMainMenu();
        Dispose();
      });
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
    ThrowIfNotReadyFor(ClientState.LoggingIn);

    Plugin.Logger.LogInfo($"[{nameof(StoryClient)}] Connecting...");
    RoomInfoPacket roomInfo = await Session.ConnectAsync();
    Plugin.Logger.LogInfo($"[{nameof(StoryClient)}] Logging in...");
    LoginResult loginResult = await Session.LoginAsync(
      Bindings.GAME,
      LoginInformation.SlotName,
      ItemsHandlingFlags.AllItems,
      new Version(0, 6, 7),
      password: LoginInformation.Password
    );

    State = ClientState.LoggingIn;
    
    // Wait for all components to enable.
    List<Task> componentEnableTasks = new();
    ItemProcessorComponents = [new StoryLevelItemProcessorClientComponent(), new TrapItemProcessorClientComponent()];
    //ModifierManagerComponent = new ArchipelagoTrapManagerClientComponent();
    foreach (ItemProcessorClientComponent itemProcessorClientComponent in ItemProcessorComponents)
    {
      componentEnableTasks.Add(itemProcessorClientComponent.Enable(Session));
    }
    //componentEnableTasks.Add(ModifierManagerComponent.Enable(Session));
    Task.WaitAll(componentEnableTasks.ToArray());

    // Process all previously received items...
    Plugin.Logger.LogInfo($"[{nameof(StoryClient)}] Receiving items...");
    State = ClientState.ReceivingItems;
    Parallel.ForEach(Session.Items.AllItemsReceived, HandleInitialItem);
    UnlockItemPatch.TryUnlockAllBossSongs();

    // We're done here. Go to level select.
    scnBase.GoToScene("scnLevelSelect");
    
    State = ClientState.LoggedIn;
    return loginResult;
  }

  /// <remarks>
  /// Run just before loading Level Select.
  /// </remarks>
  internal void ProcessNewReceivedItems()
  {
    if (State != ClientState.Ready)
      throw new InvalidOperationException("Not ready to process new items yet");

    while (Session!.Items.Any())
    {
      ItemInfo itemInfo = Session!.Items.DequeueItem();
      Plugin.Logger.LogDebug($"[{nameof(StoryClient)}] Processing item {itemInfo.ItemName} ({itemInfo.ItemId})");

      foreach (ItemProcessorClientComponent itemProcessorComponent in ItemProcessorComponents)
      {
        Plugin.Logger.LogDebug($"[{nameof(StoryClient)}] Processing item {itemInfo.ItemName} ({itemInfo.ItemId})");
        if (itemProcessorComponent.HandleItem(itemInfo))
          break;
      }
    }
  }

  /// <remarks>
  /// Levels (if not prior item) and entrances are handled in <see cref="UnlockItemPatch.UnlockBonusItemsPatch"/>.
  /// </remarks>
  private void HandleItem(ItemInfo itemInfo)
  {
    static void SetBestRank(ReadOnlyCollection<long> locations, Level level)
    {
      Rank GetBestRankForStandardLevel()
      {
        Plugin.Logger.LogInfo($"[{nameof(StoryClient)}] Handling level");

        BaseStage levelStage = Bindings.LevelToStage[level];
        (Rank, long)[] stageLocationIds;

        switch (levelStage)
        {
          case RegularStage regularStage:
            stageLocationIds = [(Rank.S, regularStage.SRankLocation), (Rank.A, regularStage.ARankLocation),
              (Rank.B, regularStage.BRankLocation)];
            break;
          case BossStage bossStage:
            stageLocationIds = bossStage.CompletePlusLocation.HasValue
              ? [(Rank.BossPerfect, bossStage.PerfectLocation), (Rank.BossNoCheckpoints, bossStage.CompletePlusLocation.Value), (Rank.BossClear, bossStage.ClearLocation)]
              : [(Rank.BossPerfect, bossStage.PerfectLocation), (Rank.BossClear, bossStage.ClearLocation)];
            break;
          default:
            throw new InvalidOperationException("Can't get best rank for this type of Stage.");
        }

        // Locations are always sent in the order of B-A-S ranks, so if we iterate in reverse we always
        //  will catch the highest rank first.
        for (int sentLocationsIndex = locations.Count - 1; sentLocationsIndex >= 0; sentLocationsIndex--)
        {
          long locationId = locations[sentLocationsIndex];

          foreach ((Rank rank, long stageLocationId) in stageLocationIds)
          {
            if (stageLocationId == locationId)
              return rank;
          }
        }
        return Rank.NotFinished;
      }

      // Level item, try to find prior rank...
      Plugin.Logger.LogInfo($"[{nameof(StoryClient)}] Attempting to get rank from locations cleared for {level}");
      if (level == Level.RhythmWeightlifter)
      {
        Plugin.Logger.LogInfo($"[{nameof(StoryClient)}] Handling Rhythm Weightlifter");

        // Rhythm Weightlifter is a special case in that it has 10 stages inside its level.
        // As the stages can only be played sequentially, and we don't have any specific Rank locations,
        //  we can take a shortcut and just set the last level unlocked to the number of
        //  Weightlifter locations we have cleared.
        int stagesCleared = locations.Count(locationId =>
          Bindings.RhythmWeightlifterStageToLocationID.Contains(locationId)
        );

        if (stagesCleared == 0)
        {
          // We haven't cleared any stages yet.
          Plugin.Logger.LogInfo($"[{nameof(StoryClient)}] Couldn't find any Rhythm Weightlifter locations");
        }
        else
        {
          Plugin.Logger.LogInfo($"[{nameof(StoryClient)}] Unlocking Rhythm Weightlifter stages up to stage {stagesCleared}");
          Persistence.SetRhythmWeightlifterLastLevelUnlocked(stagesCleared);
        }
      }
      else
      {
        Persistence.SetLevelRank(level, GetBestRankForStandardLevel());
      }
    }

    switch (State)
    {
      case ClientState.ReceivingItems:
        if (Bindings.ItemIdToLevel.TryGetValue(itemInfo.ItemId, out Level level))
          SetBestRank(Session!.Locations.AllLocationsChecked, level);
        //if (Bindings.ModifierItemIdToModifierUid.TryGetValue(itemInfo.ItemId, out string uid))
        //  ModifierManager;
          break;
    }
  }

  private void HandleInitialItem(ItemInfo itemInfo)
  {
    
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
        // no requirements, default state
        break;
      case ClientState.LoggingIn:
        if (Session is null)
        {
          Plugin.Logger.LogError($"[{nameof(StoryClient)}] Cannot log in without creating session beforehand");
          throw new InvalidOperationException("Cannot log in without creating session beforehand");
        }
        break;
      case ClientState.ReceivingItems:
        goto case ClientState.LoggingIn;
      case ClientState.LoggedIn:
        goto case ClientState.ReceivingItems;
      case ClientState.Ready:
        goto case ClientState.LoggedIn;
    }
  }

  public void Dispose()
  {
    State = ClientState.Disposed;
    throw new NotImplementedException();
  }

  public ValueTask DisposeAsync()
  {
    State = ClientState.Disposed;
    throw new NotImplementedException();
  }
}
