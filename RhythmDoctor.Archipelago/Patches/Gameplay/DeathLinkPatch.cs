namespace RhythmDoctor.Archipelago.Patches.Gameplay;

[HarmonyPatch]
internal static class DeathLinkPatch
{
  internal static bool enabled = true;

  [HarmonyPatch(typeof(LevelBase), nameof(LevelBase), MethodType.Constructor)]
  [HarmonyPrefix]
  private static void ResetDeathLinkPatch()
  {
    Plugin.Logger.LogDebug($"[{nameof(DeathLinkPatch)}] Enabling DeathLink patch");
    enabled = true;
  }

  [HarmonyPatch(typeof(RowEntity), nameof(RowEntity.CrackAdvance))]
  [HarmonyPostfix]
  private static void SendDeathLinkOnCrackedHeartPatch(RowEntity __instance)
  {
    if (Plugin.Client.DeathLinkComponent is null || !enabled)
      return;

    if (__instance.rowMisses < __instance.game.currentLevel.missesToCrackHeart)
      return;

    Plugin.Logger.LogInfo($"[{nameof(DeathLinkPatch)}] Sending {nameof(RowEntity.CrackAdvance)} death");
    enabled = false;
    Plugin.ClientOld.SendDeathLink();
  }

  [HarmonyPatch(typeof(scnGame), nameof(scnGame.FailLevel))]
  [HarmonyPostfix]
  private static void SendDeathLinkOnLevelFailPatch()
  {
    if (!enabled)
      return;

    // TODO: We could have character-specific fail lines here?
    Plugin.Logger.LogInfo($"[{nameof(DeathLinkPatch)}] Sending {nameof(scnGame.FailLevel)} death");
    enabled = false;
    Plugin.Client.DeathLinkComponent?.SendDeathLink();
  }

  // currently only used by 7-X/7-X2
  [HarmonyPatch(typeof(scnGame), nameof(scnGame.FailLevelLite))]
  [HarmonyPostfix]
  private static void SendDeathLinkOnLevelFailLitePatch(scnGame __instance)
  {
    if (!enabled)
      return;

    if (__instance.levelIdentifier == "Montage") // 7-X, fake/forced game over
      return;

    Plugin.Logger.LogInfo($"[{nameof(DeathLinkPatch)}] Sending {nameof(scnGame.FailLevelLite)} death");

    enabled = false;
    Plugin.ClientOld.SendDeathLink();
  }

  [HarmonyPatch(typeof(LevelBase), nameof(LevelBase.RunTag))]
  [HarmonyPostfix]
  private static void SendDeathLinkOnBeansHopperLossPatch(string tag)
  {
    if (
      enabled
      && scnGame.internalIdentifier == nameof(Level.BeansHopper)
      && tag == "miss"
      // Rank.passed bugs, considers F+ rank to be passed???
      && scnGame.instance.currentLevel.i1 < 30 // as per logic shown in Bar 45, Beat 1, Row 5 to get B rank
    )
    {
      Plugin.Logger.LogInfo($"[{nameof(DeathLinkPatch)}] Sending Beans Hopper death");
      enabled = false;
      // TODO: Some kind of visual indication that you failed Beans would be nice
      Plugin.ClientOld.SendDeathLink();
    }
  }
}
