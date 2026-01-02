namespace RhythmDoctor.Archipelago.Patches.Gameplay;

[HarmonyPatch(typeof(scnLevelSelect))]
internal static class LevelSelectVisualFixesPatch
{
  private static readonly Level[] ForceDisplayAsRegularLevel =
  [
    Level.GongXi, // 1-CNY
    Level.Halloween, // 1-BOO
    Level.BeansHopper, // 2-B1
    Level.Bitterness, // 2-XN
    Level.RhythmWeightlifter, // 5-B1 - doesn't count towards boss unlock but has progression locations
    Level.ArtExercise, // X-1
  ];

  // csharpier-ignore
  private static readonly Level[] ForceDisplayAsBossLevel =
  [
    Level.Boss2 // 2-X
  ];

  [HarmonyPatch(nameof(scnLevelSelect.ShowRanksText))]
  [HarmonyPrefix]
  private static void ModifyLabelVisibilitiesPatch(int index, scnLevelSelect __instance)
  {
    if (__instance.selectableEntities[index] is not SelectableCharacter character)
      return;

    if (!character.levels.TryGetValue(__instance.currentDifficulty, out Level level))
      return;

    if (ForceDisplayAsRegularLevel.Contains(level))
    {
      character.levelType = LevelType.Regular;
    }
    else if (ForceDisplayAsBossLevel.Contains(level))
    {
      character.levelType = LevelType.Boss;
    }
  }
}
