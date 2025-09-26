namespace RhythmDoctor.Archipelago.Patches.Gameplay;

using RhythmWeightlifter;

[HarmonyPatch(typeof(scnRhythmWeightlifter))]
internal static class RhythmWeightlifterPatch
{
  [HarmonyPatch(nameof(scnRhythmWeightlifter.PlayLevelDialogue))]
  [HarmonyPrefix]
  private static void DoNotShowDialoguePatch(ref bool __runOriginal)
  {
    __runOriginal = false;
  }
}
