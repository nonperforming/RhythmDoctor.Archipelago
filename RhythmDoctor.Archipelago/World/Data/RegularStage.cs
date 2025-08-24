namespace RhythmDoctor.Archipelago.World.Data;

internal class RegularStage : BaseStage
{
  internal RegularStage(Act act, long? bRankLocation, long? aRankLocation, long sRankLocation)
  {
    StageAct = act;
    BRankLocation = bRankLocation;
    ARankLocation = aRankLocation;
    SRankLocation = sRankLocation;
  }

  internal long? BRankLocation { get; init; }
  internal long? ARankLocation { get; init; }
  internal long SRankLocation { get; init; }

  internal override IReadOnlyCollection<long> GetLocationsToClear(Rank rank)
  {
    Plugin.Logger.LogDebug($"Getting locations to clear for rank {rank.internalValue}");

    // Higher ranks match first so we can `goto` lower ranks
    // TODO: probably use array for this
    List<long> ids = new();
    switch (rank)
    {
      // S Rank+
      case Rank.Sminus:
      case Rank.S:
      case Rank.Splus:
        // TODO: Need to check if clearing this with S-rank setting off breaks clearing locations
        ids.Add(SRankLocation);
        goto case Rank.A;
      // A Rank+
      case Rank.Aminus:
      case Rank.A:
      case Rank.Aplus:
        if (ARankLocation.HasValue)
        {
          ids.Add(ARankLocation.Value);
        }
        goto case Rank.B;
      // B Rank+
      case Rank.Bminus:
      case Rank.B:
      case Rank.Bplus:
        if (BRankLocation.HasValue)
        {
          ids.Add(BRankLocation.Value);
        }
        break;
      // If none of thees hit we didn't get a high enough rank to clear location
    }

    return ids.AsReadOnly();
  }
}
