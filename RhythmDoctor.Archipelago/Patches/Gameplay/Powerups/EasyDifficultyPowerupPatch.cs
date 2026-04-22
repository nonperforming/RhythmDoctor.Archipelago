namespace RhythmDoctor.Archipelago.Patches.Gameplay.Powerups;

internal class EasyDifficultyPowerupPatch : ArchipelagoModifier<EasyDifficultyPowerupPatch>
{
  public override string Uid => $"{MyPluginInfo.PLUGIN_GUID}.mod.easydifficulty";
  public override string LocalizationKey => "traps.archipelago.easyDifficultyPowerup";

  public override Type[]? ActivePatches => [typeof(ActivePatch), typeof(LockDifficultyPatch)];

  [HarmonyPatch]
  private static class ActivePatch
  {
    [HarmonyPatch(typeof(Persistence), nameof(Persistence.GetDefibrillatorP1))]
    [HarmonyPatch(typeof(Persistence), nameof(Persistence.GetDefibrillatorP2))]
    [HarmonyPrefix]
    private static void ForceEasyDifficultyPatch(ref DefibMode __result, ref bool __runOriginal)
    {
      __runOriginal = false;
      __result = DefibMode.Easy;
    }
  }
}
