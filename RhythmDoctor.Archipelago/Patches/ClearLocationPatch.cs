namespace RhythmDoctor.Archipelago.Patches;

[HarmonyPatch(typeof(HUD))]
public class ClearLocationPatch
{
  [HarmonyPatch(nameof(HUD.ShowAndSaveRank))]
  [HarmonyPrefix]
  internal static void Prefix(bool bossLevelFailed = false, bool onlySavePersistence = false)
  {
    // Is onlySavePersistence is currently only used in custom levels?
    // "there's a function for custom levels to skip the rank text [rank screen] at the end"
    // "intended to be used to make your own custom rank screen"
    // Relevant scripts:
    // global::HUD.AdvanceGameover(bool) L41:
    // `bool skipRankText = base.game.currentLevel.skipRankText;`
    // global::HUD.AdvanceGameover(bool) L48:
    // `ShowAndSaveRank(bossLevelFailed: false, skipRankText);`

    if (Plugin.Client == null || bossLevelFailed)
      return;

    if (!Enum.TryParse(scnGame.instance.levelIdentifier, out Level internalLevelName))
    {
      Plugin.Logger?.LogWarning($"Couldn't find Level. Level identifier: {scnGame.instance.levelIdentifier}");
      return;
    }

    LevelStage stage = LevelHelper.InternalToFriendlyNameDictionary[internalLevelName];
    Rank rank = scnGame.instance.currentLevel.GetRankFromMistakes();
    Plugin.Logger?.LogDebug($"Stage to clear: {stage.ToString()} with rank {rank.ToString()}");

    Plugin.Client?.locations.SendLocation(stage, rank);
  }
}
