namespace RhythmDoctor.Archipelago.Patches;

/// <summary>
/// Skips all tutorials
/// </summary>
[HarmonyPatch(typeof(scnGame))]
internal static class SkipTutorialPatch
{
  [HarmonyPatch(nameof(scnGame.Start))]
  [HarmonyPostfix]
  static void Postfix()
  {
    Plugin.Logger?.LogDebug($"Level type is {scnGame.instance.currentLevel.levelType}");

    if (scnGame.instance.currentLevel.levelType != LevelType.Tutorial)
    {
      Plugin.Logger?.LogDebug($"Not skipping {scnGame.instance.levelIdentifier}");
      return;
    }

    // If we skip the onboarding cutscene
    Plugin.Logger?.LogDebug($"Skipping {scnGame.instance.levelIdentifier}");
    scnGame.instance.SkipLevel();
  }
}
