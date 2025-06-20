namespace RhythmDoctor.Archipelago.Client;

internal sealed class TrapManager
{
  internal List<ITrap> activeTraps = new();
  internal List<ITrap> queuedTraps = new();

  private Harmony harmony = new(Plugin.TrapPatchesID);

  /// <summary>
  /// Add a trap to the active or queued list.
  /// </summary>
  /// <param name="trap">The trap to add</param>
  internal void AddTrap(ITrap trap)
  {
    if (trap.IsIncompatibleWith(activeTraps))
    {
      Plugin.Logger.LogInfo($"Queueing trap {trap.Name}");
      queuedTraps.Add(trap);
    }
    else
    {
      Plugin.Logger.LogInfo($"Added active trap {trap.Name}");
      activeTraps.Add(trap);
      harmony.PatchAll(trap.GetType());
    }
  }

  /// <summary>
  /// Clears the active traps and fills it with queued traps, then applies active traps patches.
  /// </summary>
  internal void GetNewTraps()
  {
    Plugin.Logger.LogInfo("Fetching new traps");
    activeTraps.Clear();
    Plugin.Logger.LogInfo("Unapplying trap patches");
    harmony.UnpatchSelf();

    // Copy to array here to circumvent "System.InvalidOperationException: Collection was modified"
    foreach (ITrap trap in queuedTraps.ToArray())
    {
      Plugin.Logger.LogDebug($"Checking {trap.Name} compatibility with active traps");
      if (!trap.IsIncompatibleWith(activeTraps))
      {
        Plugin.Logger.LogDebug($"{trap.Name} is compatible, moved to active traps");
        activeTraps.Add(trap);
        queuedTraps.Remove(trap);
      }
    }

    Plugin.Logger.LogInfo("Applying active trap patches");
    foreach (ITrap trap in activeTraps)
    {
      Plugin.Logger.LogDebug($"Applying {trap.Name} patch");
      harmony.PatchAll(trap.GetType());
    }
  }

  internal void ClearAllTraps(bool immediately = false)
  {
    activeTraps.Clear();
    queuedTraps.Clear();
    if (immediately)
    {
      harmony.UnpatchSelf();
    }
  }
}
