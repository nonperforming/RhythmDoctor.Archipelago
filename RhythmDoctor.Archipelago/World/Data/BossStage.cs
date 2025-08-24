namespace RhythmDoctor.Archipelago.World.Data;

internal class BossStage : BaseStage
{
  internal BossStage(
    Act act,
    long clearLocation,
    long? completePlusLocation,
    long perfectLocation,
    Dictionary<string, long>? extraLocations = null
  )
  {
    StageAct = act;
    ClearLocation = clearLocation;
    CompletePlusLocation = completePlusLocation;
    PerfectLocation = perfectLocation;
    ExtraLocations = extraLocations;
  }

  internal long ClearLocation { get; init; }
  internal long? CompletePlusLocation { get; init; }
  internal long PerfectLocation { get; init; }

  /// <summary>
  /// Extra locations (currently only used for <see cref="Level.Lesmis"/> for 3-DOG).
  /// </summary>
  internal Dictionary<string, long>? ExtraLocations { get; init; }

  internal override IReadOnlyCollection<long> GetLocationsToClear(Rank rank)
  {
    Plugin.Logger.LogDebug($"Getting locations to clear for rank {rank.internalValue}");

    // Higher ranks match first so we can `goto` lower ranks
    // TODO: probably use array for this
    List<long> ids = new();
    switch (rank)
    {
      case Rank.Splus:
      case Rank.BossPerfect:
        ids.Add(PerfectLocation);
        goto case Rank.BossNoCheckpoints;
      case Rank.BossNoCheckpoints:
        if (CompletePlusLocation.HasValue)
        {
          ids.Add(CompletePlusLocation.Value);
        }
        goto case Rank.BossClear;
      case Rank.BossClear:
        ids.Add(ClearLocation);
        break;
      default:
        throw new ArgumentOutOfRangeException($"Given rank ({rank.internalValue}) out of range");
    }

    return ids.AsReadOnly();
  }
}
