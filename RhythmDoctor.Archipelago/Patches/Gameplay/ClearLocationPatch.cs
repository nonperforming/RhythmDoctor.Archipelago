namespace RhythmDoctor.Archipelago.Patches.Gameplay;

static class ClearLocationPatch
{
  /// <summary>
  /// Loading a level.
  /// TODO: Is this used for 3-X?
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

    if (bossLevelFailed)
      return;

    if (!Enum.TryParse(scnGame.instance.levelIdentifier, out Level internalLevelName))
    {
      Plugin.Logger.LogError($"Couldn't find Level. Level identifier: {scnGame.instance.levelIdentifier}");
      return;
    }

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
      Plugin.Client.trapManager.GetNewTraps();
      #if DEBUG
      Plugin.DebugMenu.trapManager.GetNewTraps();
      #endif
    }
  }

  [HarmonyPatch(typeof(scnLevelSelect), nameof(scnLevelSelect.CheckForCutscene))]
  [HarmonyPrefix]
  static void DoNotPlayLevelUnlockCutscenePrefix(ref bool __runOriginal)
  {
    __runOriginal = false;
  }
}
