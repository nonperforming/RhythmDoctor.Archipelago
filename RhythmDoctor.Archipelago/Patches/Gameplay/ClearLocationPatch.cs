namespace RhythmDoctor.Archipelago.Patches.Gameplay;

// This patch should only be applied after the Client is created.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

internal static class ClearLocationPatch
{
  /// <summary>
  /// Loading a level.
  /// </summary>
  // [HarmonyPatch(typeof(LevelBase), nameof(LevelBase.LoadLevelAsset))]
  // [HarmonyPrefix]
  // static void ScoutLocationChecks()
  // {
  //   throw new NotImplementedException();
  //   // TODO: implement
  //   //long[] ids = Plugin.Client.locations.GetIDsForStage(stage, Rank.Splus);
  // }
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
    if (Plugin.Client.Slot.endGoal == SlotData.EndGoal.HelpingHands && level == Level.HelpingHands && rank.passed)
    {
      Plugin.Logger.LogInfo("Setting goal achieved");
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
}
