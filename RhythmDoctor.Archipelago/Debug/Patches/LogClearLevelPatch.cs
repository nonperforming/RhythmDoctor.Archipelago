#if DEBUG
namespace RhythmDoctor.Archipelago.Debug.Patches;

[HarmonyPatch]
internal static class LogClearLevelPatch
{
  [HarmonyPatch(typeof(HUD), nameof(HUD.ShowAndSaveRank))]
  [HarmonyPrefix]
  internal static void ShowAndSaveRank(bool bossLevelFailed = false, bool onlySavePersistence = false)
  {
    Plugin.Logger?.LogInfo(
      @$"--- HUD.ShowAndSaveRank(bool bossLevelFailed = {bossLevelFailed}, bool onlySavePersistence = {onlySavePersistence})
Rank: {scnGame.instance.currentLevel.GetRankFromMistakes().ToString()}
Mistakes: {scnGame.instance.mistakesManager.mistakes}
Level ID: {scnGame.instance.levelIdentifier}
---"
    );

    if (!Enum.TryParse(scnGame.instance.levelIdentifier, out Level internalLevelName))
    {
      Plugin.Logger?.LogWarning($"Couldn't find Level. Level identifier: {scnGame.instance.levelIdentifier}");
      return;
    }

    LevelStage levelStage = InternalToFriendlyName.InternalNameDictionary[internalLevelName];
    Plugin.Logger?.LogDebug($"Stage to clear: {levelStage.ToString()}");
  }

  [HarmonyPatch(typeof(HUD), nameof(HUD.AdvanceGameover))]
  [HarmonyPrefix]
  internal static void AdvanceGameover(HUD __instance, bool isPlayer = false)
  {
    Plugin.Logger?.LogInfo(
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
