#if DEBUG
namespace RhythmDoctor.Archipelago.Debug.Patches;

[HarmonyPatch(typeof(Rankscreen))]
internal static class LogClearLevelPatch
{
  [HarmonyPatch(nameof(Rankscreen.ShowAndSaveRank))]
  [HarmonyPrefix]
  private static void LogShowAndSaveRankPatch(bool bossLevelFailed, bool onlySavePersistence)
  {
    Plugin.Logger.LogInfo(
      @$"--- Rankscreen.ShowAndSaveRank(bool bossLevelFailed = {bossLevelFailed}, bool onlySavePersistence = {onlySavePersistence})
Rank: {scnGame.instance.currentLevel.GetRankFromMistakes().ToString()}
Mistakes: {scnGame.instance.mistakesManager.mistakes}
Level ID: {scnGame.instance.levelIdentifier} (internal: {scnGame.internalIdentifier})
---"
    );
  }

  [HarmonyPatch(typeof(Rankscreen), nameof(Rankscreen.AdvanceGameover))]
  [HarmonyPrefix]
  private static void LogAdvanceGameoverPatch(Rankscreen __instance, bool isPlayer)
  {
    Plugin.Logger.LogInfo(
      $@"--- Rankscreen.AdvanceGameover(bool isPlayer = {isPlayer})
State: {__instance.trueGameover}
Cutscene: {scnGame.levelToLoadSource == LevelSource.CutscenesPath}
Custom: {scnGame.instance.currentLevel.customGameoverDescription}
Skip Rank Text: {scnGame.instance.currentLevel.skipRankText}
---"
    );
  }
}
#endif
