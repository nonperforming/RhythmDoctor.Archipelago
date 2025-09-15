namespace RhythmDoctor.Archipelago.Patches.Gameplay;

/// <summary>
/// Patch to not play cutscenes.
/// </summary>
[HarmonyPatch(typeof(Persistence))]
internal static class SkipCutscenePatch
{
  /// <summary>
  /// Skip playing the passed level cutscene, post Act 2, post Act 3, post Act 4, and pre Act 5 cutscenes.
  /// </summary>
  /// <remarks>
  /// Despite these methods being six IL instructions long (14 bytes large for the smallest),
  /// these methods are somehow not inlined by JIT.
  /// </remarks>
  /// <seealso cref="LevelSelectDoNotPlayCutscenePatch"/>
  /// <param name="__result">The return value of the method.</param>
  /// <param name="__runOriginal">Whether to run the original method or not.</param>
  [HarmonyPatch(nameof(Persistence.GetPlayedPassedLevelCutscene))]
  [HarmonyPatch(nameof(Persistence.GetPlayedPostAct2Cutscene))]
  [HarmonyPatch(nameof(Persistence.GetPlayedPostAct3Cutscene))]
  [HarmonyPatch(nameof(Persistence.GetPlayedPostAct4Cutscene))]
  [HarmonyPatch(nameof(Persistence.GetPlayedPreAct5Cutscene))]
  [HarmonyPrefix]
  private static void PersistenceDoNotPlayCutscenePatch(ref bool __result, ref bool __runOriginal)
  {
    __result = true;
    __runOriginal = false;
  }

  /// <summary>
  /// Do not move Paige to the Vending Machine (for the post-Act 3 cutscene), play the Act 5 intro,
  /// or check for cutscenes to play.
  /// </summary>
  /// <seealso cref="PersistenceDoNotPlayCutscenePatch"/>
  /// <param name="__runOriginal">Whether to run the original method or not.</param>
  [HarmonyPatch(typeof(scnLevelSelect), nameof(scnLevelSelect.MovePaigeToVendingMachine))]
  [HarmonyPatch(typeof(scnLevelSelect), nameof(scnLevelSelect.PlayAct5Intro))]
  [HarmonyPatch(typeof(scnLevelSelect), nameof(scnLevelSelect.CheckForCutscene))]
  [HarmonyPrefix]
  private static void LevelSelectDoNotPlayCutscenePatch(ref bool __runOriginal)
  {
    __runOriginal = false;
  }
}
