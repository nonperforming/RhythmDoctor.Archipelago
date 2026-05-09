namespace RhythmDoctor.Archipelago.Patches.Gameplay.Traps;

internal class IceSpeedTrapPatch : ModifierPatch<IceSpeedTrapPatch>, IModifier, IArchipelagoModifier
{
  public string Uid => $"{MyPluginInfo.PLUGIN_GUID}.mod.iceSpeed";
  public string LocalizationKey => "mods.archipelago.trap.iceSpeed";
  public ModifierCompatibility Compatibility => ModifierCompatibilityBuilder.GetDefaultCompatibilityForMod(this);
  public ModifierCapability[] Capabilities => [ModifierCapability.Speed];

  public override Type[] PreviewPatches => [typeof(PreviewPatch)];
  public override Type[] ActivePatches => [];

  public float GetScale(int num, out int consumed) => Scales.BinaryScale(num, out consumed);

  [HarmonyPatch(typeof(HeartMonitor))]
  private static class PreviewPatch
  {
    [HarmonyPatch(nameof(HeartMonitor.Update))]
    [HarmonyPrefix]
    private static void ForceLevelSpeed(HeartMonitor __instance)
    {
      __instance.isSpeedOptionShown = false;
      __instance.currentLevelSpeedIndex = 0;
      __instance.speedSettings[0].phoneScreen.SetActive(true);
      __instance.speedSettings[1].phoneScreen.SetActive(false);
      __instance.speedSettings[2].phoneScreen.SetActive(false);
      __instance.speedSettingIce.Play();
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
