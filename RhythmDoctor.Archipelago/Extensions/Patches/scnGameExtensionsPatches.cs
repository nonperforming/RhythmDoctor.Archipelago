namespace RhythmDoctor.Archipelago.Extensions;

[HarmonyPatch]
internal static partial class scnGameExtensions
{
  [HarmonyPatch(typeof(scnGame), nameof(scnGame.FlashBorderFeedback), typeof(scnGame.BorderFeedbackType), typeof(Row))]
  [HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
  internal static void FlashBorderFeedbackWithDuration(
    this scnGame @this,
    scnGame.BorderFeedbackType type,
    float duration,
    Row rowSource = null!
  )
  {
#pragma warning disable CS8321 // Local function is declared but never used
    IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
#pragma warning restore CS8321 // Local function is declared but never used
    {
      return new CodeMatcher(
        // Correct the old instructions to point to `rowSource` as we are injecting `duration` into [2] its position
        instructions
          .Manipulator(item => item.opcode == OpCodes.Ldarg_2, item => item.opcode = OpCodes.Ldarg_3)
          .ToList()
      ).MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, 0.5f)).SetOpcodeAndAdvance(OpCodes.Ldarg_2) // duration
      .MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, 0.125f)).SetOpcodeAndAdvance(OpCodes.Ldarg_2) // duration
      .InstructionEnumeration();
    }

    // trick the compiler
    _ = Transpiler(null!);
  }
}
