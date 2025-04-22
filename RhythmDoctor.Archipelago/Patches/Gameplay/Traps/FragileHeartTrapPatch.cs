namespace RhythmDoctor.Archipelago.Patches.Gameplay.Traps;

[HarmonyPatch(typeof(MistakesManager))]
static class FragileHeartTrapPatch
{
  [HarmonyPatch(nameof(MistakesManager.AddMistake))]
  [HarmonyPrefix]
  static void DoubleMistakeWeight(ref float weight)
  {
    weight *= 2;
  }
}
