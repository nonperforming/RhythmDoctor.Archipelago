namespace RhythmDoctor.Archipelago.Patches.Gameplay.Powerups;

internal class EasyDifficultyPowerupPatch : ModifierPatch<EasyDifficultyPowerupPatch>, IModifier, IArchipelagoModifier
{
  public string Uid => $"{MyPluginInfo.PLUGIN_GUID}.mod.easyDifficulty";
  public string LocalizationKey => "mods.archipelago.powerup.easyDifficulty";
  public ModifierCompatibility Compatibility => ModifierCompatibilityBuilder.GetDefaultCompatibilityForMod(this);
  public ModifierCapability[] Capabilities => [ModifierCapability.Difficulty];

  public override Type[] PreviewPatches => [];
  public override Type[] ActivePatches => [typeof(ActivePatch), typeof(LockDifficultyPatch)];

  public float GetScale(int num, out int consumed) => Scales.BinaryScale(num, out consumed);

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
