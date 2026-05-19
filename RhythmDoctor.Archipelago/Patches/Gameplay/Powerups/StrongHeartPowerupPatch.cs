namespace RhythmDoctor.Archipelago.Patches.Gameplay.Powerups;

internal class StrongHeartPowerupPatch : ITrap
{
  private Harmony _harmony = null!;

  public string Name => "Strong Heart";
  public IEnumerable<Type> IncompatibleWithTraps => [typeof(FragileHeartTrapPatch)];
  public IEnumerable<Level> IncompatibleWithLevels =>
    LevelExtensions.AllBonusLevels.Concat(LevelExtensions.AllIntermissionLevels).Concat(LevelExtensions.AllBossLevels);

#pragma warning disable RCS1168
  public bool Compatible(Level _)
#pragma warning restore RCS1168
  {
    // 0 < 2 True (able to add one Strong Heart trap, 0.5x mistake weight)
    // 1 < 2 True (able to add another Strong Heart trap, 0.25x mistake weight)
    // 2 < 2 False (do not add more Strong Heart traps)
    return Plugin.ClientOld.TrapManager.Traps.OfType<StrongHeartPowerupPatch>().Count() < 2;
  }

  public void InQueue()
  {
    _harmony = new Harmony($"{Plugin.PATCH_ID_TRAP}.{nameof(StrongHeartPowerupPatch)}");
  }

  public void Active()
  {
    _harmony.PatchAll(typeof(ActivePatch));
  }

  public void ActiveEnd()
  {
    _harmony.UnpatchSelf();
  }

  [HarmonyPatch(typeof(MistakesManager))]
  private static class ActivePatch
  {
    [HarmonyPatch(nameof(MistakesManager.AddMistake))]
    [HarmonyPrefix]
    private static void HalfMistakeWeightPatch(ref float weight)
    {
      weight /= 2;
    }
  }
}
