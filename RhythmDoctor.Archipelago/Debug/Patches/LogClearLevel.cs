namespace RhythmDoctor.Archipelago.Debug.Patches;

[HarmonyPatch]
internal static class LogClearLevel
{
  [HarmonyPatch(typeof(HUD), nameof(HUD.ShowAndSaveRank))]
  [HarmonyPrefix]
  internal static void ShowAndSaveRank(bool bossLevelFailed = false, bool onlySavePersistence = false)
  {
    Plugin.Logger.LogInfo(@$"--- HUD.ShowAndSaveRank(bool bossLevelFailed = {bossLevelFailed}, bool onlySavePersistence = {onlySavePersistence})
Rank: {scnGame.instance.currentLevel.GetRankFromMistakes().ToString()}
Mistakes: {scnGame.instance.mistakesManager.mistakes}
Level ID: {scnGame.instance.levelIdentifier}
---");
  }

  [HarmonyPatch(typeof(HUD), nameof(HUD.AdvanceGameover))]
  [HarmonyPrefix]
  internal static void AdvanceGameover(HUD __instance, bool isPlayer = false)
  {
    Plugin.Logger.LogInfo($@"--- HUD.AdvanceGameover(bool isPlayer = {isPlayer})
State: {trueGameover(__instance)}
Cutscene: {scnGame.levelToLoadSource == LevelSource.CutscenesPath}
Custom: {scnGame.instance.currentLevel.customGameover}
Skip Rank Text: {scnGame.instance.currentLevel.skipRankText}
---");
  }

  [HarmonyPatch(typeof(HUD), "trueGameover", MethodType.Getter)]
  [HarmonyReversePatch]
  public static int trueGameover(HUD instance)
    => throw new NotImplementedException("Stub method called");
}
