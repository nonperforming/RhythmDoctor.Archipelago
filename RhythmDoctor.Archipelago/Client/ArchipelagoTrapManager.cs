namespace RhythmDoctor.Archipelago.Client;

internal sealed class ArchipelagoTrapManager : ModifierManagerBase, IDisposable
{
  public ArchipelagoTrapManager(Dictionary<string, int>? clearedTraps = null)
  {
    Events.Instance.LevelDeselected += OnLevelDeselected;
    ClearedTraps = clearedTraps ?? new Dictionary<string, int>();
    _gettingTrapClearedSemaphore = new SemaphoreSlim(1, 1);
  }

  private readonly Dictionary<string, int> _remoteTrapClearCache = new();

  /// <summary>
  /// Ordered array of traps that were applied to the level preview, by the lowest index to the highest index.
  /// </summary>
  /// <remarks>
  /// Can be promoted to <see cref="_activeTraps"/> if the level is started, or discarded
  /// (i.e. after invoking <see cref="ClearPreviewTraps"/>), but never null.
  /// </remarks>
  /// <seealso cref="_activeTraps"/>
#if DEBUG
  // ReSharper disable once InconsistentNaming
  internal (int index, ITrap trap)[] _previewTraps = [];
#else
  private (int index, ITrap trap)[] _previewTraps = [];
#endif
  /// <summary>
  /// Ordered array of traps that were applied to the level, by the lowest index to the highest index.
  /// Cleared by <see cref="ClearLocationPatch"/> after a location is cleared,
  /// or used by <see cref="ModifierManagerPatch.RestoreActiveTrapsOnAbandonPatch"/> to restore active traps back to the
  /// main <see cref="Traps"/> queue if no location was cleared.
  /// </summary>
  /// <remarks>
  /// Can be empty (i.e. after clearing a level which invokes <see cref="ClearActiveTraps"/>) but never null.
  /// </remarks>
  /// <example>
  /// (0, <see cref="ChilliSpeedTrapPatch"/>) -> Before <see cref="ChilliSpeedTrapPatch"/> was active, it was at
  /// position 0 of <see cref="Traps"/>.
  /// (5, <see cref="GhostTapTrapPatch"/>) -> Before <see cref="GhostTapTrapPatch"/> was active, it was at
  /// position 5 of <see cref="Traps"/>.
  /// </example>
  /// <seealso cref="_previewTraps"/>
#if DEBUG
  // ReSharper disable once InconsistentNaming
  internal (int index, ITrap trap)[] _activeTraps = [];
#else
  private (int index, ITrap trap)[] _activeTraps = [];
#endif

  /// <summary>
  /// Ordered list of traps based on when we received the trap item.
  /// </summary>
  internal readonly List<ITrap> Traps = new();

  /// <summary>
  /// Traps that will always be applied wherever possible.
  /// </summary>
  /// <remarks>
  /// Sticky traps should be applied before the standard trap queue.
  /// </remarks>
  internal readonly List<ITrap> StickyTraps = new();

  /// <summary>
  /// The traps previously cleared.
  /// </summary>
  internal readonly Dictionary<string, int> ClearedTraps;

  // Initializing here will lead to an immediate deadlock on login - no thread can ever get a semaphore.
  private readonly SemaphoreSlim _gettingTrapClearedSemaphore;

  /// <summary>
  /// Add a trap to the list of queued traps.
  /// </summary>
  /// <param name="type">The type of trap to create and add</param>
  internal async Task AddTrap(Type type)
  {
    Plugin.Logger.LogInfo($"[{nameof(ArchipelagoTrapManager)}] Creating {type.Name} trap from type");
    await AddTrap((ITrap)Activator.CreateInstance(type));
  }

  /// <summary>
  /// Add a trap to the list of queued traps.
  /// </summary>
  /// <param name="trap">The trap to add.</param>
  /// <param name="doNotCheckPriorClear">Whether to check for a prior clear with a connected Archipelago server. If false, trap is always added.</param>
  internal async Task AddTrap(ITrap trap, bool doNotCheckPriorClear = false)
  {
    // BUG: Yes, local cache doesn't work properly.
    //      When receiving a trap, the client will poll AP regardless of local cache.
    //      But, I'm rewriting the trap system already... so this will have to do for now

    async Task<bool> CheckIfTrapAlreadyCleared(string trapName)
    {
      if (doNotCheckPriorClear)
        return false;

      Plugin.Logger.LogInfo($"[{nameof(ArchipelagoTrapManager)}] Checking if {trapName} has been cleared previously...");

      int local = ClearedTraps[trapName];
      _remoteTrapClearCache.TryGetValue(trapName, out int remoteCache);

      if (remoteCache != 0 && local <= remoteCache)
      {
        Plugin.Logger.LogDebug(
          $"[{nameof(ArchipelagoTrapManager)}] {trapName} already cleared: l:{local} <= c:{_remoteTrapClearCache[trapName]} && c != 0"
        );
        return true;
      }
      else
      {
        Plugin.Logger.LogInfo($"[{nameof(ArchipelagoTrapManager)}] Waiting for semaphore to check DataStorage for {trapName}...");
        await _gettingTrapClearedSemaphore.WaitAsync();
        try
        {
          Plugin.Logger.LogInfo($"[{nameof(ArchipelagoTrapManager)}] Got semaphore, checking DataStorage for {trapName}...");

          // https://stackoverflow.com/a/11191070
          Task<int> getTrapClearsRemote = Plugin.ClientOld.Session!.DataStorage[Scope.Slot, trapName].GetAsync<int>();
          if (
            await Task.WhenAny(getTrapClearsRemote, Task.Delay(Configuration.GetRemoteTrapClearsTimeout()))
            == getTrapClearsRemote
          )
          {
            // Completed within timeout, reawait task in case it faulted
            await getTrapClearsRemote;
            remoteCache = await getTrapClearsRemote;
          }
          else
          {
            // Timed out
            Plugin.Logger.LogError(
              $"[{nameof(ArchipelagoTrapManager)}] Getting {trapName} clear status timed out "
                + $"({Configuration.GetRemoteTrapClearsTimeout()}ms) - "
                + $"setting {remoteCache} to last known good value or 0."
            );
            remoteCache = Math.Max(0, remoteCache);
          }

          if (remoteCache != 0 && local <= remoteCache)
          {
            _remoteTrapClearCache[trapName] = remoteCache;
            Plugin.Logger.LogDebug(
              $"[{nameof(ArchipelagoTrapManager)}] {trapName} already cleared: l:{local} <= r:{remoteCache} != 0 (updated cache)"
            );
            return true;
          }
          Plugin.Logger.LogInfo(
            $"[{nameof(ArchipelagoTrapManager)}] {trapName} not cleared: l:{local} <= r:{remoteCache} != 0 (updated cache)"
          );
        }
        catch (Exception exception)
        {
          Plugin.Logger.LogError(exception);
        }
        finally
        {
          Plugin.Logger.LogDebug($"[{nameof(ArchipelagoTrapManager)}] Releasing semaphore (used for {trapName})");
          _gettingTrapClearedSemaphore.Release();
        }
      }

      Plugin.Logger.LogDebug($"[{nameof(ArchipelagoTrapManager)}] {trapName} not cleared: l:{local} <= r:{remoteCache} != 0");
      return false;
    }

    if (await CheckIfTrapAlreadyCleared(trap.Name))
    {
      Plugin.Logger.LogInfo($"[{nameof(ArchipelagoTrapManager)}] Skipping {trap.Name} trap as it has been cleared previously");
      ClearedTraps[trap.Name]++;
      return;
    }

    Plugin.Logger.LogInfo($"[{nameof(TrapManager)}] Adding {trap.Name} trap as we haven't cleared it yet");
    Traps.Add(trap);
    trap.InQueue();
  }

  internal void AddStickyTraps(params string[] traps)
  {
    // TODO: better way of doing this.
    //       build from trap names, do not hard code
    ITrap GetTrapFromName(string name)
    {
      switch (name)
      {
        case "Scramble Characters":
          return new ScrambleCharactersTrapPatch();
        case "Scramble Beatsounds":
          return new ScrambleBeatsoundsTrapPatch();
        case "Scramble Hitsounds":
          return new ScrambleHitsoundsTrapPatch();
        case "Ghost Tap":
          return new GhostTapTrapPatch();
        default:
          throw new NotImplementedException();
      }
    }

    foreach (string trapName in traps)
    {
      Plugin.Logger.LogDebug($"[{nameof(ArchipelagoTrapManager)}] Adding sticky trap {trapName}");
      ITrap trap = GetTrapFromName(trapName);
      trap.InQueue();
      StickyTraps.Add(trap);
    }
  }

  internal bool IsTrapActive(string trapName)
  {
    foreach ((int _, ITrap trap) in Plugin.StoryClient.ModifierManager._activeTraps)
    {
      if (trap.Name == trapName)
      {
        return true;
      }
    }
    return false;
  }

  private void ClearTrapsList(ref (int index, ITrap trap)[] trapList, bool returnToQueue)
  {
    Plugin.Logger.LogInfo($"[{nameof(ArchipelagoTrapManager)}] Clearing traps list (return to queue: {returnToQueue})");

    if (returnToQueue)
    {
      Plugin.Logger.LogInfo($"[{nameof(ArchipelagoTrapManager)}] Restoring {trapList.Length} traps back into the trap queue");

      foreach ((int index, ITrap trap) in trapList)
      {
        // Do not return sticky traps.
        if (index == -1)
        {
          Plugin.Logger.LogDebug($"[{nameof(TrapManager)}] Trap {trap.Name} is sticky, not returning.");
          continue;
        }

        Traps.Insert(index, trap);
      }
    }

    trapList = [];
  }

  #region Preview traps
  private (int index, ITrap trap)[] PopApplicableTraps(Level level)
  {
    Plugin.Logger.LogDebug($"[{nameof(TrapManager)}] Getting applicable traps for level {level}");

    List<(int index, ITrap trap)> okTraps = new();

    foreach (ITrap trap in StickyTraps)
    {
      Plugin.Logger.LogDebug($"[{nameof(TrapManager)}] Checking sticky trap {trap.Name}");
      if (trap.IsIncompatibleWith(okTraps, level))
      {
        continue;
      }

      Plugin.Logger.LogDebug($"[{nameof(TrapManager)}] Adding sticky trap {trap.Name}");
      okTraps.Add((-1, trap));
    }

    for (int i = 0; i < Traps.Count; i++)
    {
      ITrap trap = Traps[i];
      Plugin.Logger.LogDebug($"[{nameof(TrapManager)}] Checking trap {trap.Name} at position {i}/{Traps.Count}");

      if (trap.IsIncompatibleWith(okTraps, level))
      {
        continue;
      }

      Plugin.Logger.LogDebug($"[{nameof(TrapManager)}] Adding trap {trap.Name} with original position {i}");
      okTraps.Add((i, trap));
    }

    // Remove traps when we are done iterating through all the other traps
    // otherwise we will skip the later traps if we are activating them.
    // if (okTraps.Count == 0)
    // {
    //  return [];
    // }

    // because Count is 1-indexed
    for (int i = okTraps.Count - 1; i >= 0; i--)
    {
      int index = okTraps[i].index;
      if (index == -1)
      {
        Plugin.Logger.LogDebug("Not removing sticky trap from main queue");
        continue;
      }

      Plugin.Logger.LogDebug($"{i}: Removing trap from main queue at {index}");
      Traps.RemoveAt(index);
    }

    _previewTraps = okTraps.ToArray();
    return _previewTraps;
  }

  internal (int index, ITrap trap)[] ApplyApplicableTraps(Level level)
  {
    Plugin.Logger.LogInfo($"Applying applicable traps for {level}");

    if (_previewTraps.Length != 0)
    {
      Plugin.Logger.LogWarning(
        $"Preview traps for {level} will be returned to queue."
          + "Applying new preview traps without previously discarding or promoting them should not be possible."
      );
      ClearTrapsList(ref _previewTraps, true);
    }

    if (_activeTraps.Length != 0)
    {
      Plugin.Logger.LogWarning(
        $"Active traps for {level} will be returned to queue. "
          + "Applying new preview traps with currently active traps should not be possible."
      );
      ClearTrapsList(ref _activeTraps, true);
    }

    _previewTraps = PopApplicableTraps(level);

    foreach ((_, ITrap trap) in _previewTraps)
    {
      Plugin.Logger.LogInfo($"Invoking preview level for {trap.Name}");
      trap.PreviewLevel();
    }

    return _previewTraps;
  }

  internal IEnumerable<string> GetPreviewTrapNames()
  {
    List<string> trapNames = new();

    foreach ((int _, ITrap trap) in _previewTraps)
    {
      trapNames.Add(trap.Name);
    }

    return trapNames.AsReadOnly();
  }

  private void ClearPreviewTraps()
  {
    if (_previewTraps.Length == 0)
    {
      Plugin.Logger.LogWarning("There must be at least one preview trap to clear.");
      return;
    }

    Plugin.Logger.LogInfo("Clearing preview traps");

    foreach ((_, ITrap trap) in _previewTraps)
    {
      Plugin.Logger.LogInfo($"Ending preview level for {trap.Name}");
      trap.PreviewLevelEnd();
    }

    ClearTrapsList(ref _previewTraps, true);
  }

  private void OnLevelDeselected(object _, EventArgs __)
  {
    ClearPreviewTraps();
  }
  #endregion

  #region Active traps
  internal void ClearActiveTraps(bool returnToQueue)
  {
    Plugin.Logger.LogInfo($"[{nameof(ArchipelagoTrapManager)}] Clearing active traps (return to queue: {returnToQueue})");

    if (_activeTraps.Length == 0)
    {
      Plugin.Logger.LogWarning($"[{nameof(ArchipelagoTrapManager)}] There must be at least one active trap to clear.");
      return;
    }

    foreach ((int index, ITrap trap) in _activeTraps)
    {
      trap.ActiveEnd();
      if (!returnToQueue && index != -1)
      {
        ClearedTraps[trap.Name]++;
        Plugin.ClientOld.Session!.DataStorage[Scope.Slot, trap.Name] = ClearedTraps[trap.Name];
      }
    }

    ClearTrapsList(ref _activeTraps, returnToQueue);
  }

  /// <summary>
  /// Promotes all preview traps to active traps,
  /// clearing the preview trap array and invoking <see cref="ITrap.Active"/>
  /// </summary>
  internal void PromotePreviewTrapsToActiveTraps()
  {
    if (_previewTraps.Length == 0)
    {
      Plugin.Logger.LogWarning(
        $"[{nameof(ArchipelagoTrapManager)}] There must be at least one trap under preview traps to promote to to an active trap,"
          + "ignoring request to promote traps"
      );
      return;
    }
    if (_activeTraps.Length != 0)
    {
      Plugin.Logger.LogError(
        $"[{nameof(ArchipelagoTrapManager)}] There must be no active traps currently applied. " + "Discarding old active traps."
      );
      ClearActiveTraps(false);
    }

    _activeTraps = _previewTraps;
    _previewTraps = [];
    foreach ((_, ITrap trap) in _activeTraps)
    {
      Plugin.Logger.LogInfo($"[{nameof(ArchipelagoTrapManager)}] Invoking active for {trap.Name}");
      trap.Active();
    }
  }
  #endregion

  public void Dispose()
  {
    Plugin.Logger.LogInfo($"[{nameof(ArchipelagoTrapManager)}] Disposing");

    Events.Instance.LevelDeselected -= OnLevelDeselected;

    _gettingTrapClearedSemaphore.Dispose();

    foreach ((int _, ITrap trap) in _activeTraps)
    {
      trap.ActiveEnd();
    }

    foreach ((int _, ITrap trap) in _previewTraps)
    {
      trap.PreviewLevelEnd();
    }
  }
}
