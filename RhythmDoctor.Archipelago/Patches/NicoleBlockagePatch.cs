namespace RhythmDoctor.Archipelago.Patches;

[HarmonyPatch(typeof(scnLevelSelect))]
internal static class NicoleBlockagePatch
{
  [HarmonyPatch(nameof(scnLevelSelect.UnlockEntrance))]
  [HarmonyPostfix]
  internal static void NicoleBlockageVisualPatch(ref scnLevelSelect __instance)
  {
    __instance.nicoleAct5Blockage.gameObject.SetActive(false);
  }

  [HarmonyPatch(nameof(scnLevelSelect.PerformEntityAction))]
  [HarmonyTranspiler]
  internal static IEnumerable<CodeInstruction> NicoleBlockageInteractionPatch(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher()
      .MatchForward(true, new CodeMatch(OpCodes.Ldstr, "GoToAthleteWard"))
      .Advance(3)
      .SetOpcodeAndAdvance(OpCodes.Nop)
      .SetOpcodeAndAdvance(OpCodes.Nop)
      .SetOpcodeAndAdvance(OpCodes.Nop)
      .SetOpcodeAndAdvance(OpCodes.Nop)
      .SetOpcodeAndAdvance(OpCodes.Nop)
      .SetOpcodeAndAdvance(OpCodes.Nop)
      .SetOpcodeAndAdvance(OpCodes.Nop)
      .SetOpcodeAndAdvance(OpCodes.Nop)
      .SetOpcodeAndAdvance(OpCodes.Ldc_I4_0)
      .InstructionEnumeration();
  }
}
