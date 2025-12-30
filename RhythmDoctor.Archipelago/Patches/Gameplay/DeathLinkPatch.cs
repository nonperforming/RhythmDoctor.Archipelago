namespace RhythmDoctor.Archipelago.Patches.Gameplay;

[HarmonyPatch]
internal static class DeathLinkPatch
{
  internal static bool enabled = true;

  [HarmonyPatch(typeof(LevelBase), nameof(LevelBase), MethodType.Constructor)]
  [HarmonyPrefix]
  private static void ResetDeathLinkPatch()
  {
    Plugin.Logger.LogDebug("Enabling DeathLink patch");
    enabled = true;
  }

  [HarmonyPatch(typeof(RowEntity), nameof(RowEntity.CrackAdvance))]
  [HarmonyPostfix]
  private static void SendDeathLinkOnCrackedHeartPatch(RowEntity __instance)
  {
    if (Plugin.Client.DeathLink == null || !enabled)
      return;

    if (__instance.rowMisses < __instance.game.currentLevel.missesToCrackHeart)
      return;

    enabled = false;
    Plugin.Client.SendDeathLink();
  }

  [HarmonyPatch(typeof(scnGame), nameof(scnGame.FailLevel))]
  [HarmonyPostfix]
  private static void SendDeathLinkOnLevelFailPatch()
  {
    // TODO: We could have character-specific fail lines here?
    enabled = false;
    Plugin.Client.SendDeathLink();
  }

  // currently only used by 7-X/7-X2
  [HarmonyPatch(typeof(scnGame), nameof(scnGame.FailLevelLite))]
  [HarmonyPostfix]
  private static void SendDeathLinkOnLevelFailLitePatch(scnGame __instance)
  {
    if (__instance.levelIdentifier == "Montage") // 7-X, fake/forced game over
      return;

    enabled = false;
    Plugin.Client.SendDeathLink();
  }

  [HarmonyPatch(typeof(LevelBase), nameof(LevelBase.RunTag))]
  [HarmonyPostfix]
  private static void SendDeathLinkOnBeansHopperLossPatch(string tag)
  {
    if (tag != "miss" || scnGame.instance.currentLevel.GetRankFromMistakes().passed)
      return;

    // TODO: Some kind of visual indication that you failed Beans would be nice
    Plugin.Client.SendDeathLink();
  }
}
