namespace RhythmDoctor.Archipelago.Patches.Gameplay;

[HarmonyPatch(typeof(scnLevelSelect))]
// ReSharper disable once InconsistentNaming
static class UnlockCNYPatch
{
  [HarmonyPatch(nameof(scnLevelSelect.LoadLevelData))]
  [HarmonyTranspiler]
  // ReSharper disable once InconsistentNaming
  static IEnumerable<CodeInstruction> UnhideCNYLevelPatch(IEnumerable<CodeInstruction> instructions)
  {
    // bool flag3 = this.CheckCNY() && Level.OrientalInsomniac.Passed();
    return new CodeMatcher(instructions)
      .MatchForward(
        false,
        new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(scnLevelSelect), nameof(scnLevelSelect.CheckCNY)))
      )
      .Advance(-1)
      .RemoveInstructions(6) // jump over checks excluding opcode ldc.i4.0 (we are overwriting this)
      .SetOpcodeAndAdvance(OpCodes.Ldc_I4_1) // force true
      .InstructionEnumeration();
  }
}
