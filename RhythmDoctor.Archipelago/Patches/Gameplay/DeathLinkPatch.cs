namespace RhythmDoctor.Archipelago.Patches.Gameplay;

[HarmonyPatch]
internal static class DeathLinkPatch
{
  private static readonly string[] DeathLinkMessages =
  [
    " couldn't defibrillate well enough",
    " was defeated by Connectifia abortus",
    " couldn't keep the beat",
    " had to go back to med school",
    " lost their ranked match",
    "'s been waiting for so long",
    " woof woof woof woof woof woof woof", // has been waiting for so long
    " is living with regrets",
    "'s dreams stopped",
    " played Falcon",
    " couldn't jump over the box of beans",
    " hit that \"Don't Save Changes\" again",
    " wishes they could write more, and care less",
  ];

  internal static bool enabled = true;

  [HarmonyPatch(typeof(LevelBase), nameof(LevelBase), MethodType.Constructor)]
  [HarmonyPrefix]
  private static void ResetPatch()
  {
    Plugin.Logger.LogDebug("Enabling DeathLink patch");
    enabled = true;
  }

  [HarmonyPatch(typeof(RowEntity), nameof(RowEntity.CrackAdvance))]
  [HarmonyPostfix]
  private static void SendDeathLinkPatchOnCrackedHeart(RowEntity __instance)
  {
    if (Plugin.Client.DeathLink == null || !enabled)
      return;

    if (__instance.rowMisses < __instance.game.currentLevel.missesToCrackHeart)
      return;

    enabled = false;

    // ReSharper disable once NullableWarningSuppressionIsUsed
    PlayerInfo player = Plugin.Client.Session!.Players.ActivePlayer;

    string message = player.Alias + DeathLinkMessages[Plugin.Random.Next(DeathLinkMessages.Length)];

    DeathLink deathLink = new(player.Alias, message);
    Plugin.Client.DeathLink?.SendDeathLink(deathLink);

    Plugin.Logger.LogInfo($"Sent death link: \"{message}\"");
  }
}
