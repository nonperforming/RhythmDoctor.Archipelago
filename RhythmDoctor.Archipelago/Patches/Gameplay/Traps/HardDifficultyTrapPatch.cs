namespace RhythmDoctor.Archipelago.Patches.Gameplay.Powerups;

internal class HardDifficultyPowerupPatch : ModifierPatch<HardDifficultyPowerupPatch>, IModifier, IArchipelagoModifier
{
  public string Uid => $"{MyPluginInfo.PLUGIN_GUID}.mod.hardDifficulty";
  public string LocalizationKey => "mods.archipelago.hardDifficultyTrap";
  public ModifierCompatibility Compatibility
    => ModifierCompatibilityBuilder.GetDefaultCompatibilityForMod(this);
  public ModifierCapability[] Capabilities => [ModifierCapability.Difficulty];

  public override Type[]? PreviewPatches => [];
  public override Type[]? ActivePatches => [typeof(ActivePatch), typeof(LockDifficultyPatch)];

  public double GetScale(int num, out int consumed)
    => Scales.BinaryScale(num, out consumed);

  [HarmonyPatch]
  private static class ActivePatch
  {
    [HarmonyPatch(typeof(Persistence), nameof(Persistence.GetDefibrillatorP1))]
    [HarmonyPatch(typeof(Persistence), nameof(Persistence.GetDefibrillatorP2))]
    [HarmonyPrefix]
    private static void ForceHardDifficultyPatch(ref DefibMode __result, ref bool __runOriginal)
    {
      __runOriginal = false;
      __result = DefibMode.Hard;
    }
  }
}
