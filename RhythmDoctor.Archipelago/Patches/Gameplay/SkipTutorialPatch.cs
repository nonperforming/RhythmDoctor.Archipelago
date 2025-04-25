namespace RhythmDoctor.Archipelago.Patches.Gameplay;

/// <summary>
/// Prevents tutorials from being loaded
/// </summary>
[HarmonyPatch(typeof(scnGame))]
static class SkipTutorialPatch
{
  // From https://github.com/Mysthaps/MyseIfRDPatches/blob/master/Main.cs#L223
  /// <summary>
  /// Fix story levels that exclusively use custom scripts not loading.
  /// </summary>
  /// <param name="instructions">IL instructions</param>
  /// <returns>Modified IL instructions</returns>
  [HarmonyPatch(nameof(scnGame.Start))]
  [HarmonyTranspiler]
  static IEnumerable<CodeInstruction> FixLesmisPatch(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
      .MatchForward(false, new CodeMatch(OpCodes.Ldstr, "Level_"))
      .Advance(3)
      .InsertAndAdvance(
        new CodeInstruction(OpCodes.Ldstr, ", Assembly-CSharp"),
        new CodeInstruction(
          OpCodes.Call,
          AccessTools.Method("System.String:Concat", [typeof(String), typeof(String)])
        )
      )
      .InstructionEnumeration();
  }

  /// <summary>
  /// Patch to force not attempting to load the tutorial.
  /// </summary>
  [HarmonyPatch(nameof(scnGame.Start))]
  [HarmonyPrefix]
  static void DoNotLoadTutorialPatch()
  {
    Plugin.Logger.LogDebug(
      $"Level {scnGame.internalIdentifier}: Forcing attemptToLoadTutorial from {scnGame.attemptToLoadTutorial} to false"
    );
    scnGame.attemptToLoadTutorial = false;
  }
}
