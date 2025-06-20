namespace RhythmDoctor.Archipelago.Patches.Gameplay.Traps;

[HarmonyPatch(typeof(HeartMonitor))]
class IceSpeedTrapPatch : ITrap
{
  public string Name => "Ice Speed";
  public Type[] IncompatibleWith => [typeof(ChilliSpeedTrapPatch), typeof(IceSpeedTrapPatch)];

  [HarmonyPatch(nameof(HeartMonitor.Update))]
  [HarmonyPrefix]
  static void ForceLevelSpeed(HeartMonitor __instance)
  {
    __instance.isSpeedOptionShown = false;
    __instance.currentLevelSpeedIndex = 0;
    __instance.speedSettings[0].phoneScreen.SetActive(true);
    __instance.speedSettingIce.Play();
  }

  [HarmonyPatch(nameof(HeartMonitor.ChangeLevelSpeed))]
  [HarmonyPrefix]
  static void DisableChangingLevelSpeed(ref bool __runOriginal)
  {
    Plugin.Logger.LogWarning("Level speed attempted to be changed, ignoring");
    __runOriginal = false;
  }
}
