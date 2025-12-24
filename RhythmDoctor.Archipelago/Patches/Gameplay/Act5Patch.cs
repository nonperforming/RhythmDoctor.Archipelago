namespace RhythmDoctor.Archipelago.Patches.Gameplay;

[HarmonyPatch(typeof(scnLevelSelect))]
internal static class Act5Patch
{
  [HarmonyPatch(nameof(scnLevelSelect.PerformEntityAction))]
  [HarmonyTranspiler]
  private static IEnumerable<CodeInstruction> NicoleBlockageInteractionPatch(IEnumerable<CodeInstruction> instructions)
  {
    // We want to force this bool to false so Nicole doesn't tell us to go to 2-1N before entering the PT Ward:
    // bool flag = currentSelectableObject.id == "GoToAthleteWard" && !Persistence.GetLevelRank(Level.CareLess).passed;
    return new CodeMatcher(instructions)
      .MatchForward(true, new CodeMatch(OpCodes.Ldstr, "GoToAthleteWard"))
      .Advance(3) // Do not skip the check that the current selectable object's id is "GoToAthleteWard"
      .RemoveInstructions(8) // Delete all other instructions because we're going to return false regardless
      .SetOpcodeAndAdvance(OpCodes.Ldc_I4_0) // Replace ceq and push false (int32 0) onto stack
      .InstructionEnumeration();
  }

  [HarmonyPatch(nameof(scnLevelSelect.Start))]
  [HarmonyPostfix]
  private static void DematerializeNicolePatch(scnLevelSelect __instance)
  {
    __instance.nicoleAct5Blockage.visible = false;
  }

  [HarmonyPatch(nameof(scnLevelSelect.PrepareAthleteWardTransition))]
  [HarmonyPrefix]
  private static void DoNotShowDreamBubblesPatch(ref bool __runOriginal)
  {
    __runOriginal = false;
  }
}
