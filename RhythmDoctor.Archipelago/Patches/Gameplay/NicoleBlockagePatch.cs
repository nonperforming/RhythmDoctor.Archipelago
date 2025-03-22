namespace RhythmDoctor.Archipelago.Patches.Gameplay;

[HarmonyPatch(typeof(scnLevelSelect))]
internal static class NicoleBlockagePatch
{
  [HarmonyPatch(nameof(scnLevelSelect.UnlockEntrance))]
  [HarmonyPostfix]
  internal static void NicoleBlockageVisualPatch(scnLevelSelect __instance)
  {
    __instance.nicoleAct5Blockage.gameObject.SetActive(false);
  }

  [HarmonyPatch(nameof(scnLevelSelect.PerformEntityAction))]
  [HarmonyTranspiler]
  internal static IEnumerable<CodeInstruction> NicoleBlockageInteractionPatch(IEnumerable<CodeInstruction> instructions)
  {
    // We want to force this bool to false so Nicole doesn't tell us to go to 2-1N before entering the PT Ward:
    // bool flag = currentSelectableObject.id == "GoToAthleteWard" && !Persistence.GetLevelRank(Level.CareLess).passed;
    return new CodeMatcher()
      .MatchForward(true, new CodeMatch(OpCodes.Ldstr, "GoToAthleteWard"))
      .Advance(3) // Do not skip the check that the current selectable object's id is "GoToAthleteWard"
      .SetOpcodeAndAdvance(OpCodes.Nop) // Delete all other instructions because we're going to return false regardless
      .SetOpcodeAndAdvance(OpCodes.Nop)
      .SetOpcodeAndAdvance(OpCodes.Nop)
      .SetOpcodeAndAdvance(OpCodes.Nop)
      .SetOpcodeAndAdvance(OpCodes.Nop)
      .SetOpcodeAndAdvance(OpCodes.Nop)
      .SetOpcodeAndAdvance(OpCodes.Nop)
      .SetOpcodeAndAdvance(OpCodes.Nop)
      .SetOpcodeAndAdvance(OpCodes.Ldc_I4_0) // Replace ceq and push false (int32 0) onto stack
      .InstructionEnumeration();
  }
}
