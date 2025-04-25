namespace RhythmDoctor.Archipelago.Patches.Gameplay.Powerups;

[HarmonyPatch(typeof(Persistence))]
static class EasyModePowerupPatch
{
  [HarmonyPatch(nameof(Persistence.GetDefibrillatorP1))]
  [HarmonyPatch(nameof(Persistence.GetDefibrillatorP2))]
  [HarmonyPrefix]
  static void ForceEasyDifficulty(ref DefibMode __result, ref bool __runOriginal)
  {
    __runOriginal = false;
    __result = DefibMode.Easy;
  }
}
