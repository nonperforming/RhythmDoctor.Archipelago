namespace RhythmDoctor.Archipelago.Client;

internal sealed class TrapManager : IDisposable
{
  public TrapManager(Dictionary<string, int>? clearedTraps = null)
  {
    Events.Instance.LevelDeselected += OnLevelDeselected;
    ClearedTraps = clearedTraps ?? new Dictionary<string, int>();
  }

  internal Dictionary<string, int> remoteTrapClearCache = new();

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
  /// or used by <see cref="TrapManagerPatch.RestoreActiveTrapsOnAbandonPatch"/> to restore active traps back to the
  /// main <see cref="Traps"/> queue if no location was cleared.
  /// </summary>
  /// <remarks>
  /// Can be empty (i.e. after clearing a level which invokes <see cref="ClearActiveTraps"/>) but never null.
  /// </remarks>
  /// <example>
  /// (0, <see cref="ChilliSpeedTrapPatch"/>) -> Before <see cref="ChilliSpeedTrapPatch"/> was active, it was at
  /// position 0 of <see cref="Traps"/>.
  /// (5, <see cref="GhostTapTrapPatch"/>) -> Before <see cref="GhostTapTrapPatch"/> was active, it was at
  /// position 0 of <see cref="Traps"/>.
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
  /// The traps previously cleared.
  /// </summary>
  internal readonly Dictionary<string, int> ClearedTraps;

  /// <summary>
  /// Add a trap to the list of queued traps.
  /// </summary>
  /// <param name="type">The type of trap to create and add</param>
  internal void AddTrap(Type type)
  {
    Plugin.Logger.LogInfo($"Creating {type.Name} trap from type");
    AddTrap((ITrap)Activator.CreateInstance(type));
  }

  /// <summary>
  /// Add a trap to the list of queued traps.
  /// </summary>
  /// <param name="trap">The trap to add.</param>
  internal void AddTrap(ITrap trap)
  {
    bool CheckIfTrapAlreadyCleared(string trapName)
    {
      int local = ClearedTraps[trapName];
      remoteTrapClearCache.TryGetValue(trapName, out int remote);

      if (remote != 0 && local <= remote)
      {
        Plugin.Logger.LogDebug($"Already cleared: l:{local} <= c:{remoteTrapClearCache[trapName]} != 0");
        return true;
      }
      else
      {
        remote = Plugin.Client.Session!.DataStorage[Scope.Slot, trapName];
        if (remote != 0 && local <= remote)
        {
          remoteTrapClearCache[trapName] = remote;
          Plugin.Logger.LogDebug($"Already cleared: l:{local} <= r:{remote} != 0 (updated cache)");
          return true;
        }
      }

      Plugin.Logger.LogDebug($"Not cleared: l:{local} <= r:{remote} != 0");
      return false;
    }

    if (CheckIfTrapAlreadyCleared(trap.Name))
    {
      Plugin.Logger.LogInfo($"Skipping {trap.Name} trap as it has been cleared previously");
      ClearedTraps[trap.Name]++;
      return;
    }

    Plugin.Logger.LogInfo($"Adding {trap.Name} trap as we haven't cleared it yet");
    Traps.Add(trap);
    trap.InQueue();
  }

  private void ClearTrapsList(ref (int index, ITrap trap)[] trapList, bool returnToQueue)
  {
    Plugin.Logger.LogInfo($"Clearing traps list (return to queue: {returnToQueue})");

    if (returnToQueue)
    {
      Plugin.Logger.LogInfo($"Restoring {trapList.Length} traps back into the trap queue");

      foreach ((int index, ITrap trap) in trapList)
      {
        Traps.Insert(index, trap);
      }
    }

    trapList = [];
  }

  #region Preview traps
  private (int index, ITrap trap)[] PopApplicableTraps(Level level)
  {
    Plugin.Logger.LogDebug($"Getting applicable traps for level {level}");

    List<(int index, ITrap trap)> okTraps = new();
    for (int i = 0; i < Traps.Count; i++)
    {
      ITrap trap = Traps[i];
      Plugin.Logger.LogDebug($"Checking trap {trap.Name} at position {i}/{Traps.Count}");

      if (trap.IsIncompatibleWith(okTraps, level))
      {
        continue;
      }

      Plugin.Logger.LogDebug($"Adding trap {trap.Name} with original position {i}");
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

  private void ClearPreviewTraps()
  {
    // FIXME: Find a more specific Exception
    // if (_previewTraps.Length == 0)
    //   throw new Exception("There must be at least one preview trap to clear.");
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
    Plugin.Logger.LogInfo($"Clearing active traps (return to queue: {returnToQueue})");

    // FIXME: Find a more specific Exception
    // if (_activeTraps.Length == 0)
    //   throw new Exception("There must be at least one active trap to clear.");

    foreach ((_, ITrap trap) in _activeTraps)
    {
      trap.ActiveEnd();
      if (!returnToQueue)
      {
        ClearedTraps[trap.Name]++;
        Plugin.Client.Session!.DataStorage[Scope.Slot, trap.Name] = ClearedTraps[trap.Name];
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
        "There must be at least one trap under preview traps to promote to to an active trap,"
          + "ignoring request to promote traps"
      );
      return;
    }
    if (_activeTraps.Length != 0)
    {
      Plugin.Logger.LogError("There must be no active traps currently applied. Discarding old active traps.");
      ClearActiveTraps(false);
    }

    _activeTraps = _previewTraps;
    _previewTraps = [];
    foreach ((_, ITrap trap) in _activeTraps)
    {
      Plugin.Logger.LogInfo($"Invoking active for {trap.Name}");
      trap.Active();
    }
  }
  #endregion

  public void Dispose()
  {
    Plugin.Logger.LogInfo("Disposing of TrapManager");

    Events.Instance.LevelDeselected -= OnLevelDeselected;

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
