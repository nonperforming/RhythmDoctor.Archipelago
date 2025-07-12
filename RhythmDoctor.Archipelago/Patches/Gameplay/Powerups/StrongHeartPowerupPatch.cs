namespace RhythmDoctor.Archipelago.Patches.Gameplay.Powerups;

class StrongHeartPowerupPatch : ITrap
{
  // ReSharper disable once NullableWarningSuppressionIsUsed
  private Harmony harmony = null!;

  public string Name => "Strong Heart";
  public Type[] IncompatibleWithTraps => [typeof(FragileHeartTrapPatch)];
  public LevelStage[] IncompatibleWithLevels => LevelStageExtensions.BonusIntermissionAndBossLevels;

  public bool Compatible()
  {
    // 0 < 2 True (able to add one Strong Heart trap, 0.5x mistake weight)
    // 1 < 2 True (able to add another Strong Heart trap, 0.25x mistake weight)
    // 2 < 2 False (do not add more Strong Heart traps)
    return Plugin.Client.trapManager.Traps.OfType<StrongHeartPowerupPatch>().Count() < 2;
  }

  public void InQueue()
  {
    harmony = new($"{Plugin.PATCH_ID_TRAP}.{nameof(StrongHeartPowerupPatch)}");
  }

  public void Active()
  {
    harmony.PatchAll(typeof(ActivePatch));
  }

  public void ActiveEnd()
  {
    harmony.UnpatchSelf();
  }

  [HarmonyPatch(typeof(MistakesManager))]
  private static class ActivePatch
  {
    [HarmonyPatch(nameof(MistakesManager.AddMistake))]
    [HarmonyPrefix]
    static void HalfMistakeWeight(ref float weight)
    {
      weight /= 2;
    }
  }
}
