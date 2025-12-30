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

    // From Rankscreen.ShowAndSaveRank(bool, bool)
    // ...
    // case LevelType.Boss:
    // {
    //   bool passedLevelWithoutCheckpoints = base.game.GetPassedLevelWithoutCheckpoints();
    //   string text2 = (isAnySRank ? "Perfect" : (passedLevelWithoutCheckpoints ? "CompletePlus" : "Complete"));
    //   customText.text = RDString.Get("rankscreen.act" + base.game.currentLevel.bossActNum + text2);
    //   customText.color = color;
    //   customText.gameObject.SetActive(value: true);
    //   if (base.game.currentLevel is Level_Lesmis { dogMode: not false })
    //   {
    //     customText.text = RDString.Get("rankscreen.dogtor");
    //   }
    //   Narration.Say(customText.text, NarrationCategory.Notification, false, NarrationActionName.ToContinue);
    //   break;
    // }
    // ...

    List<long> ids = new();

    // S ranks are always equivalent to Perfect.
    if (rank.perfected)
    {
      ids.Add(PerfectLocation);
    }

    if (CompletePlusLocation.HasValue && rank.noCheckpoints)
    {
      // Check if we've cleared without checkpoints.
      if (scnGame.instance.GetPassedLevelWithoutCheckpoints())
      {
        ids.Add(CompletePlusLocation.Value);
      }
    }

    if (rank.passed)
    {
      ids.Add(ClearLocation);
    }

    return ids.AsReadOnly();
  }
}
