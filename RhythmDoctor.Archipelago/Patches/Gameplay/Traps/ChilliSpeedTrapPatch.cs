namespace RhythmDoctor.Archipelago.Patches.Gameplay.Traps;

[HarmonyPatch(typeof(HeartMonitor))]
internal static class ChilliSpeedTrapPatch
{
  [HarmonyPatch(nameof(HeartMonitor.Update))]
  [HarmonyPrefix]
  internal static void ForceLevelSpeed(HeartMonitor __instance)
  {
    __instance.isSpeedOptionShown = false;
    __instance.currentLevelSpeedIndex = 2;
    __instance.speedSettings[2].phoneScreen.SetActive(true);
    __instance.speedSettingChilli.Play();
  }

  [HarmonyPatch(nameof(HeartMonitor.ChangeLevelSpeed))]
  [HarmonyPrefix]
  static void DisableChangingLevelSpeed(ref bool __runOriginal)
  {
    Plugin.Logger.LogWarning("Level speed attempted to be changed, ignoring");
    __runOriginal = false;
  }
}
