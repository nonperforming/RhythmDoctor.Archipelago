namespace RhythmDoctor.Archipelago.Patches.Shared;

[HarmonyPatch(typeof(PauseMenu), nameof(PauseMenu.Update))]
internal static class LockDifficultyPatch
{
  [HarmonyPatch]
  [HarmonyPrefix]
  private static void Patch(ref PauseMenu __instance)
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
            GameObject glitchEffect = UnityEngine.Object.Instantiate(content.glitchesPrefab, content.transform.parent);
            content.glitches = glitchEffect.GetComponentsInChildren<SpriteAnimation>();
          }
        }
      }
    }
  }
}
