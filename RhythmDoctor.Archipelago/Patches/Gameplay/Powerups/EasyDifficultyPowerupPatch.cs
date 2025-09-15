namespace RhythmDoctor.Archipelago.Patches.Gameplay.Powerups;

internal class EasyDifficultyPowerupPatch : ITrap
{
  // ReSharper disable once NullableWarningSuppressionIsUsed
  private Harmony _harmony = null!;

  public string Name => "Easy Mode";
  public IEnumerable<Type> IncompatibleWithTraps =>
    [typeof(EasyDifficultyPowerupPatch), typeof(HardDifficultyTrapPatch)];

  public void InQueue()
  {
    _harmony = new Harmony($"{Plugin.PATCH_ID_TRAP}.{nameof(EasyDifficultyPowerupPatch)}");
  }

  public void Active()
  {
    _harmony.PatchAll(typeof(ActivePatch));

    // TODO: Lock the difficulty seen in the settings menu
  }

  public void ActiveEnd()
  {
    _harmony.UnpatchSelf();

    // TODO: Unlock the difficulty seen in the settings menu
  }

  [HarmonyPatch(typeof(Persistence))]
  private static class ActivePatch
  {
    [HarmonyPatch(nameof(Persistence.GetDefibrillatorP1))]
    [HarmonyPatch(nameof(Persistence.GetDefibrillatorP2))]
    [HarmonyPrefix]
    private static void ForceEasyDifficultyPatch(ref DefibMode __result, ref bool __runOriginal)
    {
      __runOriginal = false;
      __result = DefibMode.Easy;
    }
  }
}
