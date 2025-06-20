namespace RhythmDoctor.Archipelago.Patches.Gameplay.Powerups;

[HarmonyPatch(typeof(Persistence))]
class EasyModePowerupPatch : ITrap
{
  public string Name => "Easy Mode";
  public Type[] IncompatibleWith => [typeof(EasyModePowerupPatch), typeof(HardModeTrapPatch)];

  // TODO: Lock the difficulty seen in the settings menu

  [HarmonyPatch(nameof(Persistence.GetDefibrillatorP1))]
  [HarmonyPatch(nameof(Persistence.GetDefibrillatorP2))]
  [HarmonyPrefix]
  static void ForceEasyDifficulty(ref DefibMode __result, ref bool __runOriginal)
  {
    __runOriginal = false;
    __result = DefibMode.Easy;
  }
}
