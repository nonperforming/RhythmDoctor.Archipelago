namespace RhythmDoctor.Archipelago.Patches.Gameplay;

[HarmonyPatch(typeof(scnLevelSelect))]
// ReSharper disable once InconsistentNaming
static class UnlockBOOPatch
{
  [HarmonyPatch(nameof(scnLevelSelect.LoadLevelData))]
  [HarmonyTranspiler]
  // ReSharper disable once InconsistentNaming
  static IEnumerable<CodeInstruction> UnhideBOOLevelPatch(IEnumerable<CodeInstruction> instructions)
  {
    // bool flag4 = RDBase.IsHalloweenWeek() && Level.OrientalInsomniac1.Passed();
    return new CodeMatcher(instructions)
      .MatchForward(true, new CodeMatch(OpCodes.Ldstr, "1-BOO"))
      .Advance(3) // do not overwrite Key == "1-BOO" check
      .RemoveInstructions(5) // jump over checks excluding opcode ldc.i4.0 (we are overwriting this)
      .SetOpcodeAndAdvance(OpCodes.Ldc_I4_1)
      .InstructionEnumeration();
  }
}
