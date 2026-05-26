using System.Collections.ObjectModel;

namespace RhythmDoctor.Archipelago.Client;

/// <summary>
/// Archipelago client.
/// </summary>
internal sealed class Client : IDisposable, IAsyncDisposable
{
  internal LoginInformation LoginInformation { get; private set; }
  internal SlotData SlotData { get; private set; }

  internal Modifiers.ModifierManagerBase ModifierManager { get; private set; };
  internal DeathLinkClientComponent? DeathLinkComponent { get; private set; }
  internal IReplicationClientComponent? ReplicationComponent { get; private set; }
  internal ClientState State { get; private set; } = ClientState.NotReady;
  internal SlotData Slot;

  internal IEnumerable<IClientComponent> ClientComponents
  {
    get
    {
      if (DeathLinkComponent != null)
        yield return DeathLinkComponent;
      if (ReplicationComponent != null)
        yield return ReplicationComponent;
    }
  }

  internal ArchipelagoSession? Session { get; private set; }

  private readonly CancellationTokenSource _cancellationTokenSource = new();

  internal Client(LoginInformation loginInformation)
  {
    LoginInformation = loginInformation;
    ModifierManager = new ArchipelagoTrapManager();
  }

  internal ArchipelagoSession CreateSession()
  {
    static void BindEvents(ArchipelagoSession session)
    {
      session.Socket.ErrorReceived += (
        (__exception, __message) =>
          Plugin.Logger.LogError($"[{nameof(Client)}] Socket error {__exception} - {__message}")
      );
      session.Socket.SocketClosed += (__reason => _ = AttemptReconnect(__reason));
      session.MessageLog.OnMessageReceived += (
        __message => Plugin.Logger.LogInfo($"[{nameof(Client)}] Received message \"{__message}\"")
      );
      session.Items.ItemReceived += ItemReceived;
      session.DataStorage[Scope.Slot, Persistence.PaigeStaysKey].OnValueChanged += ReplicatePaigeStays;
      session.DataStorage[Scope.Slot, Persistence.IanDesktopLoginKey].OnValueChanged += ReplicateIansDesktopUnlocked;
    }

    ThrowIfNotReadyFor(ClientState.CreatingSession);
    State = ClientState.CreatingSession;
    Plugin.Logger.LogInfo($"[{nameof(Client)}] Creating Archipelago session to {LoginInformation.Uri}");

    Session = ArchipelagoSessionFactory.CreateSession(LoginInformation.Uri);
    BindEvents(Session);
    State = ClientState.CreatedSession;

    return Session;
  }

  /// <summary>
  /// Log into the Archipelago server with the <see cref="LoginInformation"/>
  /// given via <see cref="Client"/>'s constructor.
  /// </summary>
  /// <remarks>
  /// <see cref="CreateSession"/> should be called prior to calling this.
  /// </remarks>
  /// <returns></returns>
  internal async Task<LoginResult> Login()
  {
    ThrowIfNotReadyFor(ClientState.LoggingIn);

    //if (State == )
    //
    Plugin.Logger.LogInfo($"[{nameof(Client)}] Logging in...");
    await Session.ConnectAsync();

    State = ClientState.LoggingIn;

    // Process all previously received items...
    Plugin.Logger.LogInfo($"[{nameof(Client)}] Receiving items...");
    State = ClientState.ReceivingItems;
    Parallel.ForEach(Session.Items.AllItemsReceived, itemInfo => HandleItem(itemInfo, true));
    UnlockItemPatch.TryUnlockAllBossSongs();

    State = ClientState.LoggedIn;
    return loginResult;
  }

  /// <remarks>
  /// Levels (if not prior item) and entrances are handled in <see cref="UnlockItemPatch.UnlockBonusItemsPatch"/>.
  /// </remarks>
  private Task HandleItem(ItemInfo itemInfo, bool priorItem = false)
  {
    static void SetBestRank(ReadOnlyCollection<long> locations, Level level)
    {
      Rank GetBestRankForStandardLevel()
      {
        Plugin.Logger.LogInfo($"[{nameof(Client)}] Handling level");

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
      Plugin.Logger.LogInfo($"[{nameof(Client)}] Attempting to get rank from locations cleared for {level}");
      if (level == Level.RhythmWeightlifter)
      {
        Plugin.Logger.LogInfo($"[{nameof(Client)}] Handling Rhythm Weightlifter");

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
          Plugin.Logger.LogInfo($"[{nameof(Client)}] Couldn't find any Rhythm Weightlifter locations");
        }
        else
        {
          Plugin.Logger.LogInfo($"[{nameof(Client)}] Unlocking Rhythm Weightlifter stages up to stage {stagesCleared}");
          Persistence.SetRhythmWeightlifterLastLevelUnlocked(stagesCleared);
        }
      }
      else
      {
        Persistence.SetLevelRank(level, GetBestRankForStandardLevel());
      }
    }

    if (priorItem)
    {
      if (Bindings.ItemIdToLevel.TryGetValue(itemInfo.ItemId, out Level level))
        SetBestRank(Session!.Locations.AllLocationsChecked, level);
      if (Bindings.TrapItemIdToLevel.TryGetValue(itemInfo.ItemId, out Type type))
    }
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
          Plugin.Logger.LogError($"[{nameof(Client)}] Cannot log in without creating session beforehand");
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
