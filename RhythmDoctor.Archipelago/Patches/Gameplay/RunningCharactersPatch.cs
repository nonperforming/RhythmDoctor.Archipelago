using System.Reflection;

namespace RhythmDoctor.Archipelago.Patches.Gameplay;

[HarmonyPatch(typeof(WalkingSelectableCharacter))]
internal static class RunningCharactersPatch
{
  /// <summary>
  /// Let <see cref="WalkingSelectableCharacter"/>s walk over to locked levels.
  /// </summary>
  /// <param name="instructions">IL instructions</param>
  /// <returns>Modified IL instructions</returns>
  [HarmonyPatch(nameof(WalkingSelectableCharacter.UpdateCharacter))]
  [HarmonyTranspiler]
  private static IEnumerable<CodeInstruction> WalkingSelectableCharacterOverLockedLevelsPatch(
    IEnumerable<CodeInstruction> instructions
  )
  {
    CodeMatcher matcher = new(instructions);

    foreach (CodeInstruction instruction in matcher.Instructions())
    {
      if (
        instruction.opcode == OpCodes.Call
        && (MethodInfo)instruction.operand
          == AccessTools.Method(typeof(RDUtils), nameof(RDUtils.Locked), [typeof(Level)])
      )
      {
        // return false
        instruction.opcode = OpCodes.Ldc_I4_0;
      }
    }

    return matcher.InstructionEnumeration();
  }
}
