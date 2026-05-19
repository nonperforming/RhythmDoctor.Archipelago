#if DEBUG
namespace RhythmDoctor.Archipelago.Debug.Patches;

[HarmonyPatch(typeof(SteamIntegration))]
internal static class DisableSteamAchievementsPatch
{
  [HarmonyPatch(nameof(SteamIntegration.UnlockAchievement), typeof(string), typeof(bool))]
  [HarmonyPrefix]
  private static void DoNotUnlockAchievementPatch(ref bool __runOriginal)
  {
    __runOriginal = false;
  }
}
#endif
