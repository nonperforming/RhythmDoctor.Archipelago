namespace RhythmDoctor.Archipelago.Patches.Gameplay;

/// <summary>
/// Prevents tutorials from being loaded
/// </summary>
[HarmonyPatch(typeof(scnGame))]
internal static class SkipTutorialPatch
{
  // From https://github.com/Mysthaps/MyseIfRDPatches/blob/master/Main.cs#L223
  /// <summary>
  /// Fix story levels that exclusively use custom scripts not loading.
  /// </summary>
  /// <param name="instructions">IL instructions</param>
  /// <returns>Modified IL instructions</returns>
  [HarmonyPatch(nameof(scnGame.Start))]
  [HarmonyTranspiler]
  private static IEnumerable<CodeInstruction> FixLesmisPatch(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
      .MatchForward(false, new CodeMatch(OpCodes.Ldstr, "Level_"))
      .Advance(3)
      .InsertAndAdvance(
        new CodeInstruction(OpCodes.Ldstr, ", Assembly-CSharp"),
        new CodeInstruction(OpCodes.Call, AccessTools.Method("System.String:Concat", [typeof(string), typeof(string)]))
      )
      .InstructionEnumeration();
  }

  /// <summary>
  /// Patch to force not attempting to load the tutorial.
  /// </summary>
  [HarmonyPatch(nameof(scnGame.Start))]
  [HarmonyPrefix]
  private static void DoNotLoadTutorialPatch()
  {
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
