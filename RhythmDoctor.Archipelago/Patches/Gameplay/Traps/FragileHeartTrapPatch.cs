namespace RhythmDoctor.Archipelago.Patches.Gameplay.Traps;

[HarmonyPatch(typeof(MistakesManager))]
class FragileHeartTrapPatch : ITrap
{
  public string Name => "Fragile Heart";
  public Type[] IncompatibleWith => [];

  [HarmonyPatch(nameof(MistakesManager.AddMistake))]
  [HarmonyPrefix]
  static void DoubleMistakeWeight(ref float weight)
  {
    weight *= 2;
  }
}
