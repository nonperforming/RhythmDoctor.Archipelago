namespace RhythmDoctor.Archipelago.Patches.Gameplay.Traps;

internal class HardDifficultyTrapPatch : ITrap
{
  // ReSharper disable once NullableWarningSuppressionIsUsed
  private Harmony _harmony = null!;

  public string Name => "Hard Mode";

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
    // TODO: Lock the difficulty seen in the settings menu
    //       like how fullscreen and resolution options are locked in window-dance levels
    [HarmonyPatch(nameof(Persistence.GetDefibrillatorP1))]
    [HarmonyPatch(nameof(Persistence.GetDefibrillatorP2))]
    [HarmonyPrefix]
    private static void ForceHardDifficultyPatch(ref DefibMode __result, ref bool __runOriginal)
    {
      __runOriginal = false;
      __result = DefibMode.Hard;
    }
  }
}
