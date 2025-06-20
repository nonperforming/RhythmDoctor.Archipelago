namespace RhythmDoctor.Archipelago.Patches.Gameplay.Powerups;

[HarmonyPatch(typeof(MistakesManager))]
class StrongHeartPowerupPatch : ITrap
{
  public string Name => "Strong Heart";
  public Type[] IncompatibleWith => [];

  [HarmonyPatch(nameof(MistakesManager.AddMistake))]
  [HarmonyPrefix]
  static void HalfMistakeWeight(ref float weight)
  {
    weight /= 2;
  }
}
