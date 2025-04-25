namespace RhythmDoctor.Archipelago.Patches.Gameplay.Traps;

[HarmonyPatch(typeof(Persistence))]
static class HardModeTrapPatch
{
  [HarmonyPatch(nameof(Persistence.GetDefibrillatorP1))]
  [HarmonyPatch(nameof(Persistence.GetDefibrillatorP2))]
  [HarmonyPrefix]
  static void ForceHardDifficulty(ref DefibMode __result, ref bool __runOriginal)
  {
    __runOriginal = false;
    __result = DefibMode.Hard;
  }
}
