#if DEBUG
namespace RhythmDoctor.Archipelago.Debug.Patches;

[HarmonyPatch(typeof(HUD))]
static class LogClearLevelPatch
{
  [HarmonyPatch(nameof(HUD.ShowAndSaveRank))]
  [HarmonyPrefix]
  static void LogShowAndSaveRankPatch(bool bossLevelFailed, bool onlySavePersistence)
  {
    Plugin.Logger.LogInfo(
      @$"--- HUD.ShowAndSaveRank(bool bossLevelFailed = {bossLevelFailed}, bool onlySavePersistence = {onlySavePersistence})
Rank: {scnGame.instance.currentLevel.GetRankFromMistakes().ToString()}
Mistakes: {scnGame.instance.mistakesManager.mistakes}
Level ID: {scnGame.instance.levelIdentifier}
---"
    );
  }

  [HarmonyPatch(typeof(HUD), nameof(HUD.AdvanceGameover))]
  [HarmonyPrefix]
  static void LogAdvanceGameoverPatch(HUD __instance, bool isPlayer)
  {
    Plugin.Logger.LogInfo(
      $@"--- HUD.AdvanceGameover(bool isPlayer = {isPlayer})
State: {__instance.trueGameover}
Cutscene: {scnGame.levelToLoadSource == LevelSource.CutscenesPath}
Custom: {scnGame.instance.currentLevel.customGameover}
Skip Rank Text: {scnGame.instance.currentLevel.skipRankText}
---"
    );
  }
}
#endif
