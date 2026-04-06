namespace RhythmDoctor.Archipelago.World.Data;

internal class BossStage : BaseStage
{
  internal BossStage(Act act, long clearLocation, long? completePlusLocation, long perfectLocation)
  {
    StageAct = act;
    ClearLocation = clearLocation;
    CompletePlusLocation = completePlusLocation;
    PerfectLocation = perfectLocation;
  }

  internal long ClearLocation { get; init; }
  internal long? CompletePlusLocation { get; init; }
  internal long PerfectLocation { get; init; }

  internal override IEnumerable<long> GetLocationsToClear(Rank rank)
  {
    Plugin.Logger.LogDebug($"Getting locations to clear for rank {rank.internalValue}");

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
    // To note: Boss stage ranks do not necessarily match up with conventional level ranks.
    // For example, one could get D rank and still pass as Complete+

    // S ranks are always equivalent to Perfect.
    if (rank.perfected)
    {
      ids.Add(PerfectLocation);
    }

    if (CompletePlusLocation.HasValue)
    {
      // Check if we've cleared without checkpoints.
      if (rank.perfected || scnGame.instance.GetPassedLevelWithoutCheckpoints())
      {
        ids.Add(CompletePlusLocation.Value);
      }
    }

    // You must reach the end of a boss level in order to clear it.
    ids.Add(ClearLocation);

    return ids.AsReadOnly();
  }
}
