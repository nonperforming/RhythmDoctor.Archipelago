namespace RhythmDoctor.Archipelago.Patches.Gameplay;

// This patch should only be applied after the Client is created.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

static class ClearLocationPatch
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
  static void CustomClearLocationPatch(bool bossLevelFailed, bool onlySavePersistence)
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
    Plugin.DebugMenu.trapManager.ClearActiveTraps(false);
#endif

    if (bossLevelFailed)
    {
      Plugin.Client.trapManager.ClearActiveTraps(false);
      return;
    }

    if (!Enum.TryParse(scnGame.instance.levelIdentifier, out Level internalLevelName))
    {
      Plugin.Logger.LogError($"Couldn't find Level. Level identifier: {scnGame.instance.levelIdentifier}");
      Plugin.Client.trapManager.ClearActiveTraps(false);
      return;
    }

    Plugin.Logger.LogDebug("Getting locations to clear");
    LevelStage stage = LevelHelper.InternalToFriendlyNameDictionary[internalLevelName];
    Rank rank = scnGame.instance.currentLevel.GetRankFromMistakes();
    long[] ids = Plugin.Client.locations.GetIDsForStage(stage, rank);
    Plugin.Logger.LogDebug(
      $"Stage to clear: {stage.ToString()} with rank {rank.ToString()} ({string.Join(", ", ids)})"
    );

    bool clearedNewLocation = false;
    foreach (long id in ids)
    {
      if (!Plugin.Client.session.Locations.AllLocationsChecked.Contains(id))
      {
        clearedNewLocation = true;
        break;
      }
    }

    if (clearedNewLocation)
    {
      Task.Run(() => Plugin.Client.locations.SendLocation(stage, rank));
      Plugin.Client.trapManager.ClearActiveTraps(false);
    }
    else
    {
      Plugin.Client.trapManager.ClearActiveTraps(true);
    }
  }

  [HarmonyPatch(typeof(scnLevelSelect), nameof(scnLevelSelect.CheckForCutscene))]
  [HarmonyPrefix]
  static void DoNotPlayLevelUnlockCutscenePatch(ref bool __runOriginal)
  {
    __runOriginal = false;
  }
}
