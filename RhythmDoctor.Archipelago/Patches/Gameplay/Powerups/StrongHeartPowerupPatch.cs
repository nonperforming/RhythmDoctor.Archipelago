namespace RhythmDoctor.Archipelago.Patches.Gameplay.Powerups;

internal class StrongHeartPowerupPatch : ModifierPatch<StrongHeartPowerupPatch>, IModifier, IArchipelagoModifier
{
  private static int strength = 2;
  
  public string Uid => $"{MyPluginInfo.PLUGIN_GUID}.mod.strongHeart";
  public string LocalizationKey => "mods.archipelago.strongHeartPowerup";
  // TODO: CACHE
  public ModifierCompatibility Compatibility => ModifierCompatibilityBuilder.GetDefaultBuilderForMod(this)
    .SetMaximumStrength(2)
    .Build();
  public ModifierCapability[] Capabilities => [];

  public override Type[]? PreviewPatches => [];
  public override Type[]? ActivePatches => [typeof(ActivePatch)];
  

  [HarmonyPatch(typeof(MistakesManager))]
  private static class ActivePatch
  {
    [HarmonyPatch(nameof(MistakesManager.AddMistake))]
    [HarmonyPrefix]
    private static void HalfMistakeWeightPatch(ref float weight)
    {
      weight /= strength;
    }
  }
}
