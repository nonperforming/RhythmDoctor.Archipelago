#if DEBUG
namespace RhythmDoctor.Archipelago.Debug.Patches;

[HarmonyPatch(typeof(scnLevelSelect))]
internal static class LogEntityInteractionsPatch
{
  [HarmonyPatch(nameof(scnLevelSelect.PerformEntityAction))]
  [HarmonyPrefix]
  private static void LogSelectableObjectInteractionsPatch(scnLevelSelect __instance)
  {
    switch (__instance.selectedEntity)
    {
      case SelectableCharacter selectableCharacter:
        Level level = selectableCharacter.levels[__instance.currentDifficulty];
        Plugin.Logger.LogInfo(
          @$"--- scnLevelSelect.PerformEntityAction() [scnLevelSelect.selectedEntity is SelectableCharacter]
Level: {level}
State: {Persistence.GetStateFromLevel(level)}
---"
        );
        break;
      case SelectableObject selectableObject:
        // TODO: Implement properly
        Plugin.Logger.LogDebug(selectableObject);
        break;
      // FIXME: ReSharper: ConvertTypeCheckPatternToNullCheck
      // What?
      case SelectableEntity selectableEntity:
        // TODO: Implement properly
        Plugin.Logger.LogDebug(selectableEntity);
        break;
    }
  }
}
#endif
