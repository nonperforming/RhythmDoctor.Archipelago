namespace RhythmDoctor.Archipelago.Patches.Gameplay.Traps;

internal class IceSpeedTrapPatch : ITrap
{
  // ReSharper disable once NullableWarningSuppressionIsUsed
  private Harmony _harmony = null!;

  public string Name => "Ice Speed";
  public IEnumerable<Type> IncompatibleWithTraps => [typeof(ChilliSpeedTrapPatch), typeof(IceSpeedTrapPatch)];

  public void InQueue()
  {
    _harmony = new Harmony($"{Plugin.PATCH_ID_TRAP}.{nameof(IceSpeedTrapPatch)}");
  }

  public void PreviewLevel()
  {
    _harmony.PatchAll(typeof(Patch));
  }

  public void PreviewLevelEnd()
  {
    _harmony.UnpatchSelf();
  }

  // Intentionally left blank
  public void Active() { }

  public void ActiveEnd()
  {
    _harmony.UnpatchSelf();
  }

  [HarmonyPatch(typeof(HeartMonitor))]
  private static class Patch
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
