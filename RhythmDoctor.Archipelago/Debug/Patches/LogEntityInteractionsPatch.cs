#if DEBUG
namespace RhythmDoctor.Archipelago.Debug.Patches;

[HarmonyPatch(typeof(scnLevelSelect))]
static class LogEntityInteractionsPatch
{
  [HarmonyPatch(nameof(scnLevelSelect.PerformEntityAction))]
  [HarmonyPrefix]
  static void LogSelectableObjectInteractionsPatch(scnLevelSelect __instance)
  {
    switch (__instance.selectedEntity)
    {
      case SelectableCharacter selectableCharacter:
        Level level = selectableCharacter.levels[__instance.currentDifficulty];
        Plugin.Logger.LogInfo(
          @$"--- scnLevelSelect.PerformEntityAction() [scnLevelSelect.selectedEntity is SelectableCharacter]
Level: {level} ({LevelHelper.InternalToFriendlyNameDictionary[level]})
State: {Persistence.GetStateFromLevel(level)}
---"
        );
        break;
      case SelectableObject selectableObject:
        throw new NotImplementedException();
      // FIXME: ReSharper: ConvertTypeCheckPatternToNullCheck
      // What?
      case SelectableEntity selectableEntity:
        throw new NotImplementedException();
    }
  }
}
#endif
