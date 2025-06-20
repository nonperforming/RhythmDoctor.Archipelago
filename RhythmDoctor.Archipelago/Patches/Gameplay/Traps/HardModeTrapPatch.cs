namespace RhythmDoctor.Archipelago.Patches.Gameplay.Traps;

[HarmonyPatch(typeof(Persistence))]
class HardModeTrapPatch : ITrap
{
  public string Name => "Hard Mode";
  public Type[] IncompatibleWith => [typeof(EasyModePowerupPatch), typeof(HardModeTrapPatch)];

  // TODO: Lock the difficulty seen in the settings menu
  //       like how fullscreen and resolution options are locked in window-dance levels
  [HarmonyPatch(nameof(Persistence.GetDefibrillatorP1))]
  [HarmonyPatch(nameof(Persistence.GetDefibrillatorP2))]
  [HarmonyPrefix]
  static void ForceHardDifficulty(ref DefibMode __result, ref bool __runOriginal)
  {
    __runOriginal = false;
    __result = DefibMode.Hard;
  }
}
