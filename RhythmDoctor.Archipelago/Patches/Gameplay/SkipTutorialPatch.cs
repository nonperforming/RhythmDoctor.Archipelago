namespace RhythmDoctor.Archipelago.Patches.Gameplay;

/// <summary>
/// Prevents tutorials from being loaded, excluding 7-X (for story reasons)
/// </summary>
[HarmonyPatch(typeof(scnGame))]
internal static class SkipTutorialPatch
{
  /// <summary>
  /// Patch to force not attempting to load the tutorial.
  /// </summary>
  [HarmonyPatch(nameof(scnGame.Start))]
  [HarmonyPrefix]
  private static void DoNotLoadTutorialPatch()
  {
    if (scnGame.internalIdentifier == nameof(Level.Montage))
    {
      Plugin.Logger.LogDebug("Level is 7-X, not skipping tutorial");
      return;
    }

    Plugin.Logger.LogDebug(
      $"Level {scnGame.internalIdentifier}: Forcing attemptToLoadTutorial from {scnGame.attemptToLoadTutorial} to false"
    );
    scnGame.attemptToLoadTutorial = false;
  }

  [HarmonyPatch(typeof(Persistence), nameof(Persistence.GetFirstTimePlaying))]
  [HarmonyPrefix]
  private static void DoNotLoadIntroPatch(ref bool __result, ref bool __runOriginal)
  {
    Plugin.Logger.LogDebug("Forcing first_time to false");
    __runOriginal = false;
    __result = false;
  }
}
