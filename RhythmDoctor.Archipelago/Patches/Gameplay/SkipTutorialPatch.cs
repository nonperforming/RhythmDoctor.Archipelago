namespace RhythmDoctor.Archipelago.Patches.Gameplay;

/// <summary>
/// Prevents tutorials from being loaded
/// FIXME: This doesn't actually work.
/// 3-X and 1-2 bug and fail to load if we patch scnGame.Start
/// even if nothing runs. Notably, these stages are mostly implemented by
/// a custom script... but for some reason other levels with custom scripts
/// such as X-WOT (custom heart, decompile Level_Unbeatable) work correctly.
/// This alternative patch should in theory prevent the tutorial from being
/// loaded outright...
/// ...but doesn't for some reason.
/// </summary>
[HarmonyPatch(typeof(scnBase))]
static class SkipTutorialPatch
{
  [HarmonyPatch(nameof(scnBase.GoToLevel))]
  [HarmonyPrefix]
  static void GoToLevelPatch(string path, bool loadGameScene, ref bool attemptToLoadTutorial)
  {
    Plugin.Logger.LogDebug($"Forcing attemptToLoadTutorial from {attemptToLoadTutorial} to false");
    attemptToLoadTutorial = false;
    scnGame.attemptToLoadTutorial = false;
  }
}
