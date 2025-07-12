namespace RhythmDoctor.Archipelago.Patches.Gameplay.Powerups;

class EasyModePowerupPatch : ITrap
{
  // ReSharper disable once NullableWarningSuppressionIsUsed
  private Harmony harmony = null!;

  public string Name => "Easy Mode";
  public Type[] IncompatibleWithTraps => [typeof(EasyModePowerupPatch), typeof(HardModeTrapPatch)];

  public void InQueue()
  {
    harmony = new($"{Plugin.PATCH_ID_TRAP}.{nameof(EasyModePowerupPatch)}");
  }

  public void Active()
  {
    harmony.PatchAll(typeof(ActivePatch));

    // TODO: Lock the difficulty seen in the settings menu
  }

  public void ActiveEnd()
  {
    harmony.UnpatchSelf();

    // TODO: Unlock the difficulty seen in the settings menu
  }

  [HarmonyPatch(typeof(Persistence))]
  private static class ActivePatch
  {
    [HarmonyPatch(nameof(Persistence.GetDefibrillatorP1))]
    [HarmonyPatch(nameof(Persistence.GetDefibrillatorP2))]
    [HarmonyPrefix]
    static void ForceEasyDifficultyPatch(ref DefibMode __result, ref bool __runOriginal)
    {
      __runOriginal = false;
      __result = DefibMode.Easy;
    }
  }
}
