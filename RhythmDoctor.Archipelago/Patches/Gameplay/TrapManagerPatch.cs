namespace RhythmDoctor.Archipelago.Patches.Gameplay;

/// <summary>
/// A <see cref="HarmonyPatch"/> that invokes <see cref="ITrap.Compatible"/>, <see cref="ITrap.Active"/> and
/// <see cref="ITrap.PreviewLevel"/> in the <see cref="Client"/>'s <see cref="TrapManager"/>'s trap queue for the
/// selected level just before entering a level, and restores them into the <see cref="TrapManager"/>'s
/// <see cref="TrapManager.Traps"/> when exiting a level without clearing a location in the <see cref="Client"/>.
/// Also invokes <see cref="ITrap.PreviewLevel"/> and <see cref="ITrap.PreviewLevelEnd"/>.
/// </summary>
/// <remarks>
/// If built in the Debug configuration, the applicable <see cref="ITrap"/>s in <see cref="DebugMenu"/>'s
/// <see cref="TrapManager"/> will be applied after the <see cref="Client"/>'s traps.
/// </remarks>
/// <seealso cref="ITrap"/>
/// <seealso cref="TrapManager"/>
[HarmonyPatch]
static class TrapManagerPatch
{
  [HarmonyPatch(typeof(scnLevelSelect), nameof(scnLevelSelect.SelectCharacter))]
  [HarmonyPrefix]
  static void ApplyApplicableTrapPreviewPatch(scnLevelSelect __instance)
  {
    if (__instance.selectedEntity is not SelectableCharacter selectableCharacter)
      return;

    Level level = selectableCharacter.levels[__instance.currentDifficulty];
    Plugin.Client.trapManager.ApplyApplicableTraps(level);

#if DEBUG
    Plugin.Logger.LogInfo($"DEBUG TRAPS: Applying applicable trap previews for level {level}");
    Plugin.DebugMenu.trapManager.ApplyApplicableTraps(level);
#endif
  }

  // PreviewEnd is managed by TrapManager using an event from Pulse.

  [HarmonyPatch(typeof(scnLevelSelect), nameof(scnLevelSelect.GoToLevelSequence))]
  [HarmonyPostfix]
  static void ApplyApplicableTrapsPatch(string levelToGo, scnLevelSelect __instance)
  {
    Plugin.Logger.LogInfo("Promoting preview traps to active traps");

    // Rhythm Dogtor and Rhythm Weightlifter dog mode can bypass selection,
    // so we may need apply preview patches here.
    if (Plugin.Client.trapManager._previewTraps.Length == 0 && levelToGo is "Lesmis" or "RhythmWeightlifter")
    {
      Plugin.Logger.LogInfo("Going to Rhythm Dogtor/Rhythm Weightlifter (dog) - applying preview traps now");
      // The cheat code for Rhythm Dogtor allows you to end it
      // hovering over any level, we need to check the levelToGo.
      Level level = RDUtils.ParseEnum(levelToGo, Level.None);
      Plugin.Client.trapManager.ApplyApplicableTraps(level);
    }

    Plugin.Client.trapManager.PromotePreviewTrapsToActiveTraps();

#if DEBUG
    Plugin.Logger.LogInfo("DEBUG: Promoting preview traps to active traps");
    Plugin.DebugMenu.trapManager.PromotePreviewTrapsToActiveTraps();
#endif
  }

  // Unactive by clearing a level is handled by ClearLocationPatch.

  [HarmonyPatch(typeof(scnGame), nameof(scnGame.Quit))]
  [HarmonyPostfix]
  static void RestoreActiveTrapsOnAbandonPatch()
  {
    Plugin.Logger.LogInfo("Clearing active traps (returning to queue)");
    Plugin.Client.trapManager.ClearActiveTraps(true);
#if DEBUG
    Plugin.Logger.LogInfo("DEBUG: Clearing active traps (do not return to queue)");
    Plugin.DebugMenu.trapManager.ClearActiveTraps(false);
#endif
  }
}
