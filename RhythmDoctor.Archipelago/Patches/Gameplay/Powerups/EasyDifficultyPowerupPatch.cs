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
  }

  public void ActiveEnd()
  {
    _harmony.UnpatchSelf();
  }

  [HarmonyPatch]
  private static class ActivePatch
  {
    [HarmonyPatch(typeof(Persistence), nameof(Persistence.GetDefibrillatorP1))]
    [HarmonyPatch(typeof(Persistence), nameof(Persistence.GetDefibrillatorP2))]
    [HarmonyPrefix]
    private static void ForceEasyDifficultyPatch(ref DefibMode __result, ref bool __runOriginal)
    {
      __runOriginal = false;
      __result = DefibMode.Easy;
    }

    [HarmonyPatch(typeof(PauseMenu), nameof(PauseMenu.Update))]
    [HarmonyPrefix]
    private static void LockDifficultyPatch(ref PauseMenu __instance)
    {
      foreach ((PauseModeName modeName, PauseMenuMode mode) in __instance.instantiatedModes)
      {
        if (modeName is not PauseModeName.GameSettings)
          continue;

        foreach (PauseMenuMode.Category category in mode.categories)
        {
          foreach ((PauseContentName contentName, PauseModeContentArrows content) in category.contentArrowsDict)
          {
            if (contentName is not (PauseContentName.DefibrillatorP1 or PauseContentName.DefibrillatorP2))
              continue;

            // From Initialize()
            Plugin.Logger.LogDebug($"Setting canChangeContentValue to false for {contentName}");
            content.canChangeContentValue = false;
            if (content.glitches == null)
            {
              GameObject glitchEffect = UnityEngine.Object.Instantiate(
                content.glitchesPrefab,
                content.transform.parent
              );
              content.glitches = glitchEffect.GetComponentsInChildren<SpriteAnimation>();
            }
          }
        }
      }
    }
  }
}
