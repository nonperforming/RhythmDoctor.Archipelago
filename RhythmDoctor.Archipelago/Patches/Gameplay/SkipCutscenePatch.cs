namespace RhythmDoctor.Archipelago.Patches.Gameplay;

/// <summary>
/// Patch to not play cutscenes.
/// </summary>
[HarmonyPatch]
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
  [HarmonyPatch(typeof(Persistence), nameof(Persistence.GetPlayedPassedLevelCutscene))]
  [HarmonyPatch(typeof(Persistence), nameof(Persistence.GetPlayedPostAct2Cutscene))]
  [HarmonyPatch(typeof(Persistence), nameof(Persistence.GetPlayedPostAct3Cutscene))]
  [HarmonyPatch(typeof(Persistence), nameof(Persistence.GetPlayedPostAct4Cutscene))]
  [HarmonyPatch(typeof(Persistence), nameof(Persistence.GetPlayedPreAct5Cutscene))]
  [HarmonyPatch(typeof(Persistence), nameof(Persistence.GetPlayedPreAct6Cutscene))]
  [HarmonyPatch(typeof(Persistence), nameof(Persistence.GetPlayedHaileyDuetIntroduction))]
  [HarmonyPatch(typeof(Persistence), nameof(Persistence.GetPlayedPreBitternessCutscene))]
  [HarmonyPatch(typeof(Persistence), nameof(Persistence.GetPlayedRooftopCutscene))]
  [HarmonyPrefix]
  private static void PersistenceDoNotPlayCutscenePatch(ref bool __result, ref bool __runOriginal)
  {
    __result = true;
    __runOriginal = false;
  }

  /// <summary>
  /// Do not move Paige to the Vending Machine (for the post-Act 3 cutscene), play the Act 5 intro,
  /// or check cutscenes to play (or play cutscenes).
  /// </summary>
  /// <seealso cref="PersistenceDoNotPlayCutscenePatch"/>
  /// <param name="__runOriginal">Whether to run the original method or not.</param>
  [HarmonyPatch(typeof(scnLevelSelect), nameof(scnLevelSelect.MovePaigeToVendingMachine))]
  [HarmonyPatch(typeof(scnLevelSelect), nameof(scnLevelSelect.PlayAct5Intro))]
  [HarmonyPatch(typeof(scnLevelSelect), nameof(scnLevelSelect.CheckForCutscene))]
  [HarmonyPatch(typeof(scnBase), nameof(scnBase.GoToCutscene))]
  [HarmonyPrefix]
  private static void LevelSelectDoNotPlayCutscenePatch(ref bool __runOriginal)
  {
    __runOriginal = false;
  }

  [HarmonyPatch(typeof(scnLevelSelect), nameof(scnLevelSelect.Start))]
  [HarmonyPostfix]
  private static void SkipStoryAndCutscenePatch(scnLevelSelect __instance)
  {
    scnLevelSelect.bitternessWarningPlayed = true;
    __instance.voidItemProgress = 3;
    __instance.selectableEntities.Find(entity => entity.id == "VoidItem1").normalEnabled = true;
    __instance.selectableEntities.Find(entity => entity.id == "VoidItem2").normalEnabled = true;
    __instance.selectableEntities.Find(entity => entity.id == "VoidItem3").normalEnabled = true;
  }
}
