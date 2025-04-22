namespace RhythmDoctor.Archipelago.Patches.Gameplay.Powerups;

[HarmonyPatch(typeof(MistakesManager))]
static class StrongHeartTrapPatch
{
  [HarmonyPatch(nameof(MistakesManager.AddMistake))]
  [HarmonyPrefix]
  static void HalfMistakeWeight(ref float weight)
  {
    weight /= 2;
  }
}
