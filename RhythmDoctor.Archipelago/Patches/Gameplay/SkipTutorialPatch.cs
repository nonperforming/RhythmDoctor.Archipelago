namespace RhythmDoctor.Archipelago.Patches.Gameplay;

/// <summary>
/// Prevents tutorials from being loaded
/// </summary>
[HarmonyPatch(typeof(scnGame))]
static class SkipTutorialPatch
{
  // From https://github.com/Mysthaps/MyseIfRDPatches/blob/master/Main.cs#L223
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
          AccessTools.Method("System.String:Concat", new Type[] { typeof(String), typeof(String) })
        )
      )
      .InstructionEnumeration();
  }

  [HarmonyPatch(nameof(scnGame.Start))]
  [HarmonyPrefix]
  static void DoNotLoadTutorialPatch()
  {
    //if (level in )
    Plugin.Logger.LogDebug(
      $"Level {scnGame.internalIdentifier}: Forcing attemptToLoadTutorial from {scnGame.attemptToLoadTutorial} to false"
    );
    scnGame.attemptToLoadTutorial = false;
  }
}
