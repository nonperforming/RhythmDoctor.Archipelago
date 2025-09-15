namespace RhythmDoctor.Archipelago.Patches.Gameplay.Traps;

internal class ChilliSpeedTrapPatch : ITrap
{
  // ReSharper disable once NullableWarningSuppressionIsUsed
  private Harmony _harmony = null!;

  public string Name => "Chilli Speed";
  public IEnumerable<Type> IncompatibleWithTraps => [typeof(ChilliSpeedTrapPatch), typeof(IceSpeedTrapPatch)];

  public void InQueue()
  {
    _harmony = new Harmony($"{Plugin.PATCH_ID_TRAP}.{nameof(ChilliSpeedTrapPatch)}");
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
