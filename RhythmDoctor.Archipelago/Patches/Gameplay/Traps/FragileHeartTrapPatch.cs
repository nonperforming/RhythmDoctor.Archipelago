namespace RhythmDoctor.Archipelago.Patches.Gameplay.Traps;

internal class FragileHeartTrapPatch : ITrap
{
  // ReSharper disable once NullableWarningSuppressionIsUsed
  private Harmony _harmony = null!;

  public string Name => "Fragile Heart";
  public IEnumerable<Type> IncompatibleWithTraps => [typeof(StrongHeartPowerupPatch)];
  public IEnumerable<Level> IncompatibleWithLevels =>
    LevelExtensions.AllBonusLevels.Concat(LevelExtensions.AllIntermissionLevels).Concat(LevelExtensions.AllBossLevels);

#pragma warning disable RCS1168
  public bool Compatible(Level _)
#pragma warning restore RCS1168
  {
    // 0 < 2 True (able to add one Fragile Heart trap, 2x mistake weight)
    // 1 < 2 True (able to add another Fragile Heart trap, 4x mistake weight)
    // 2 < 2 False (do not add more Fragile Heart traps)
    return Plugin.ClientOld.TrapManager.Traps.OfType<FragileHeartTrapPatch>().Count() < 2;
  }

  public void InQueue()
  {
    _harmony = new Harmony($"{Plugin.PATCH_ID_TRAP}.{nameof(FragileHeartTrapPatch)}");
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
    private static void DoubleMistakeWeightPatch(ref float weight)
    {
      weight *= 2;
    }
  }
}
