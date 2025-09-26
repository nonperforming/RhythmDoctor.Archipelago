namespace RhythmDoctor.Archipelago.Patches.Gameplay;

// This patch should only be applied after the Client is created.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

[HarmonyPatch]
internal static class ClearLocationPatch
{
  // [HarmonyPatch(typeof(LevelBase), nameof(LevelBase.LoadLevelAsset))]
  // [HarmonyPrefix]
  // private static void ScoutLocationChecks()
  // {
  //   // TODO: Implement.
  //   // Get the locations we will clear so we can show the user
  //   // the locations they clear when they pass the level (replace the rank text)
  // }

  /// <summary>
  /// Loading a level.
  /// </summary>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if end goal is not valid.</exception>
  [HarmonyPatch(typeof(HUD), nameof(HUD.ShowAndSaveRank))]
  [HarmonyPrefix]
  private static void CustomClearLocationPatch(bool bossLevelFailed, bool onlySavePersistence)
  {
    // Is onlySavePersistence is currently only used in custom levels?
    // "there's a function for custom levels to skip the rank text [rank screen] at the end"
    // "intended to be used to make your own custom rank screen"
    // Relevant scripts:
    // global::HUD.AdvanceGameover(bool) L41:
    // `bool skipRankText = base.game.currentLevel.skipRankText;`
    // global::HUD.AdvanceGameover(bool) L48:
    // `ShowAndSaveRank(bossLevelFailed: false, skipRankText);`
    // TODO: Boss levels and said "custom levels" will overwrite virtual LevelBase.ShowGameOver
    //       Check for HUD.base.game.currentLevel.customGameover!!! When is this case applicable?

#if DEBUG
    // Discard Debug Menu traps regardless of result.
    Plugin.DebugMenu.TrapManager.ClearActiveTraps(false);
#endif

    if (bossLevelFailed)
    {
      Plugin.Client.TrapManager.ClearActiveTraps(false);
      return;
    }

    if (!Enum.TryParse(scnGame.instance.levelIdentifier, out Level level))
    {
      Plugin.Logger.LogError($"Couldn't find Level. Level identifier: {scnGame.instance.levelIdentifier}");
      Plugin.Client.TrapManager.ClearActiveTraps(false);
      return;
    }

    Plugin.Logger.LogDebug("Getting locations to clear");
    Rank rank = scnGame.instance.currentLevel.GetRankFromMistakes();

    IReadOnlyCollection<long> ids;
    if (scnGame.instance.currentLevel.dogMode && level == Level.Lesmis)
    {
      Plugin.Logger.LogInfo("Detected Rhythm Dogtor");
      // Playing 3-DOG - Rhythm Dogtor
      // ReSharper disable once NullableWarningSuppressionIsUsed
      BossStage rhythmDogtor = (Bindings.LevelToStage[level] as BossStage)!;

      List<long> idsToClear = [rhythmDogtor.ExtraLocations["dog_clear"]];
      if (rank.perfected)
      {
        idsToClear.Add(rhythmDogtor.ExtraLocations["dog_perfect"]);
      }

      ids = idsToClear.AsReadOnly();
    }
    else
    {
      ids = Bindings.LevelToStage[level].GetLocationsToClear(rank);
      Plugin.Logger.LogDebug($"Stage to clear: {level} with rank {rank} ({string.Join(", ", ids)})");
    }

    // X-0 - Helping Hands end goal
    bool clearedAll = false;
    if (Plugin.Client.Slot.endGoal == SlotData.EndGoal.HelpingHands && level == Level.HelpingHands && rank.passed)
    {
      Plugin.Logger.LogInfo("Setting goal achieved - Helping Hands");
      Plugin.Client.Session.SetGoalAchieved();
    }
    else if (
      (
        Plugin.Client.Slot.endGoal == SlotData.EndGoal.ARankAll
        || Plugin.Client.Slot.endGoal == SlotData.EndGoal.BRankAll
        || Plugin.Client.Slot.endGoal == SlotData.EndGoal.PerfectAll
      )
      && Plugin.Client.Slot.endGoal != SlotData.EndGoal.HelpingHands
    )
    {
      clearedAll = true;
      Rank minimumRank;
      switch (Plugin.Client.Slot.endGoal)
      {
        case SlotData.EndGoal.PerfectAll:
          minimumRank = Rank.S;
          break;
        case SlotData.EndGoal.ARankAll:
          minimumRank = Rank.A;
          break;
        case SlotData.EndGoal.BRankAll:
          minimumRank = Rank.B;
          break;
        default:
          throw new ArgumentOutOfRangeException($"End Goal ({Plugin.Client.Slot.endGoal}) not valid value.");
      }
      foreach (Level otherLevel in Enum.GetValues(typeof(Level)))
      {
        Rank otherRank = Persistence.GetLevelRank(otherLevel);
        // If we aren't above the minimum rank, bail.
        if (minimumRank <= otherRank.ToNormal())
        {
          clearedAll = false;
          break;
        }
      }
    }

    if (clearedAll)
    {
      Plugin.Logger.LogInfo("Setting goal achieved - Cleared all");
      Plugin.Client.Session.SetGoalAchieved();
    }

    bool clearedNewLocation = false;
    foreach (long id in ids)
    {
      if (Plugin.Client.Session.Locations.AllLocationsChecked.Contains(id))
      {
        continue;
      }
      clearedNewLocation = true;
      break;
    }

    if (clearedNewLocation)
    {
      // FIXME: This blocks until completion - should be async!
      long[] locationsToClear = ids.ToArray();
      Plugin.Client.Session.Locations.CompleteLocationChecks(locationsToClear);
      Plugin.Client.TrapManager.ClearActiveTraps(false);
    }
    else
    {
      Plugin.Client.TrapManager.ClearActiveTraps(true);
    }
  }

  [HarmonyPatch(typeof(RhythmWeightlifter.Level), nameof(RhythmWeightlifter.Level.GetRank))]
  [HarmonyPostfix]
  private static void RhythmWeightlifterClearLocationPatch(string __result)
  {
    if (__result == "-")
    {
      // We haven't actually cleared the level yet.
      return;
    }
    // TODO: Show what item we have sent out somehow.
    Plugin.Client.Session.Locations.CompleteLocationChecks(
      Bindings.RhythmWeightlifterStageToLocationID[RhythmWeightlifter.scnRhythmWeightlifter.gameInstance.LevelIndex]
    );
  }
}
