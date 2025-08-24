namespace RhythmDoctor.Archipelago.Patches.Gameplay.Traps;

class FragileHeartTrapPatch : ITrap
{
  // ReSharper disable once NullableWarningSuppressionIsUsed
  private Harmony harmony = null!;

  public string Name => "Fragile Heart";
  public IEnumerable<Type> IncompatibleWithTraps => [typeof(StrongHeartPowerupPatch)];

  public bool Compatible()
  {
    // 0 < 2 True (able to add one Fragile Heart trap, 2x mistake weight)
    // 1 < 2 True (able to add another Fragile Heart trap, 4x mistake weight)
    // 2 < 2 False (do not add more Fragile Heart traps)
    return Plugin.Client.trapManager.Traps.OfType<FragileHeartTrapPatch>().Count() < 2;
  }

  public void InQueue()
  {
    harmony = new($"{Plugin.PATCH_ID_TRAP}.{nameof(FragileHeartTrapPatch)}");
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
    static void DoubleMistakeWeightPatch(ref float weight)
    {
      weight *= 2;
    }
  }
}
