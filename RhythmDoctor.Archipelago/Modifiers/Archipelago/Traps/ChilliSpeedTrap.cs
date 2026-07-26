namespace RhythmDoctor.Archipelago.Modifiers.Archipelago.Traps;

internal class ChilliSpeedTrap : ModifierPatch<ChilliSpeedTrap>, IModifier, IArchipelagoModifier
{
  public string Uid => $"{MyPluginInfo.PLUGIN_GUID}.mod.chilliSpeed";
  public string LocalizationKey => "mods.archipelago.trap.chilliSpeed";
  public ModifierCompatibility Compatibility => ModifierCompatibilityBuilder.GetDefaultCompatibilityForMod(this);
  public ModifierCapability[] Capabilities => [ModifierCapability.Speed];

  public override Type[] PreviewPatches => [typeof(PreviewPatch)];
  public override Type[] ActivePatches => [];

  public IScale Scale => BinaryScale.Instance;

  [HarmonyPatch(typeof(HeartMonitor))]
  private static class PreviewPatch
  {
    [HarmonyPatch(nameof(HeartMonitor.Update))]
    [HarmonyPrefix]
    private static void ForceLevelSpeedPatch(HeartMonitor __instance)
    {
      __instance.isSpeedOptionShown = false;
      __instance.currentLevelSpeedIndex = 2;
      __instance.speedSettings[0].phoneScreen.SetActive(false);
      __instance.speedSettings[1].phoneScreen.SetActive(false);
      __instance.speedSettings[2].phoneScreen.SetActive(true);
      __instance.speedSettingChilli.Play();
    }

    [HarmonyPatch(nameof(HeartMonitor.ChangeLevelSpeed))]
    [HarmonyPrefix]
    private static void DisableChangingLevelSpeedPatch(ref bool __runOriginal)
    {
      Plugin.Logger.LogWarning("Level speed attempted to be changed, ignoring");
      __runOriginal = false;
    }
  }
}
