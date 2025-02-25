namespace RhythmDoctor.Archipelago.World;

internal class Locations
{
  private LocationsData _data;

  public Locations()
  {
    _data = new();
  }

  internal long[] GetIDsForStage(LevelStage stage, Rank rank)
  {
    // TODO: Is there a better way we could do this?
    List<long> ids = new(3);
    Region region = LevelHelper.ActToRegionDictionary[LevelHelper.LevelToActDictionary[stage]];
    long offset = _data.Locations[region][stage][0].ID;

    // Boss levels are handled differently from regular levels.
    // Note that bonus levels (excluding Rhythm Weightlifter) have a rank much like regular levels, so they are considered as such.
    if (stage == LevelStage.RhythmWeightlifter)
    {
      throw new NotImplementedException();
    }
    else if (LevelHelper.IsBoss(stage))
    {
      // Higher ranks match first so we can `goto` lower ranks
      switch (rank)
      {
        case Rank.BossPerfect:
          if (LevelHelper.HasCheckpoints(stage))
            ids.Add(offset + 2);
          else
            ids.Add(offset + 1);

          goto case Rank.BossNoCheckpoints;
        case Rank.BossNoCheckpoints:
          if (!LevelHelper.HasCheckpoints(stage))
            goto case Rank.BossClear;

          ids.Add(offset + 1);
          goto case Rank.BossClear;
        case Rank.BossClear:
          ids.Add(offset);
          break;
        default:
          // Failed boss stage
          break;
      }
    }
    else
    {
      // Higher ranks match first so we can `goto` lower ranks
      switch (rank)
      {
        // S Rank+
        case Rank.Sminus:
        case Rank.S:
        case Rank.Splus:
          ids.Add(offset + 2);
          goto case Rank.A;
        // A Rank+
        case Rank.Aminus:
        case Rank.A:
        case Rank.Aplus:
          ids.Add(offset + 1);
          goto case Rank.B;
        // B Rank+
        case Rank.Bminus:
        case Rank.B:
        case Rank.Bplus:
          ids.Add(offset);
          break;
        default:
          // Didn't get a high enough rank to clear location
          break;
      }
    }

#if DEBUG
    // Sanity checks
    foreach (long id in ids)
    {
      Plugin.Logger.LogDebug($"Checking ID: {id}");
      if (id < 82_104_121_68_114_000 || id > 82_104_121_68_114_999)
      {
        Plugin.Logger.LogError($"ID {id} is out of range");
      }
    }
#endif

    return ids.ToArray();
  }

  /// <summary>
  /// Sends a location to be cleared.
  /// </summary>
  /// <param name="stage">The stage played</param>
  /// <param name="rank">The rank achieved</param>
  /// <exception cref="NullReferenceException">Client is not connected/null, or locations data is not populated</exception>
  internal async Task SendLocation(LevelStage stage, Rank rank)
  {
    if (Plugin.Client == null || Plugin.Client.session == null)
    {
      throw new NullReferenceException("Client is not connected/null");
    }
    if (Plugin.Client.locations == null)
    {
      throw new NullReferenceException("Locations data is not populated");
    }

    long[] ids = GetIDsForStage(stage, rank);
    Plugin.Logger.LogInfo($"Sending location IDs {string.Join(", ", ids)}");
    await Plugin.Client.session.Locations.ScoutLocationsAsync(ids);

    await Plugin.Client.session.Locations.CompleteLocationChecksAsync(ids);
  }
}
