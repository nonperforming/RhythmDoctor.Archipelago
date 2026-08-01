namespace RhythmDoctor.Archipelago.Patches.Gameplay;

/// <seealso cref="IModifier"/>
/// <seealso cref="ArchipelagoModifierManagerClientComponent"/>
/// <seealso cref="ModifierManagerBase"/>
[HarmonyPatch]
internal static class ArchipelagoModifierManagerPatch
{
  [HarmonyPatch(typeof(scnLevelSelect), nameof(scnLevelSelect.SelectCharacter))]
  [HarmonyPrefix]
  private static void ApplyApplicableTrapPreviewPatch(scnLevelSelect __instance)
  {
    if (__instance.selectedEntity is not SelectableCharacter selectableCharacter)
      return;

    Level level = selectableCharacter.levels[__instance.currentDifficulty];
    Plugin.StoryClient.ModifierManagerComponent!.PushAndApplyCompatibleTrapsInQueue(level);
  }

  [HarmonyPatch(typeof(HeartMonitor), nameof(HeartMonitor.Show))]
  [HarmonyPostfix]
  private static void ShowTrapNameOnPhonePatch(HeartMonitor __instance)
  {
    IEnumerable<string> previewTraps = Plugin.StoryClient.ModifierManagerComponent!.GetPreviewTrapNames();
    if (!previewTraps.Any())
      return;

    Plugin.Logger.LogDebug($"[{nameof(ArchipelagoModifierManagerPatch)}] Instantiating guest credit for preview traps");
    __instance.isGuestCreditShown = true;

    foreach (string trapName in previewTraps)
    {
      Plugin.Logger.LogDebug($"Creating guest credit for {trapName}");
      GuestData guestData = new()
      {
        type = null,
        link = null,
        // FIXME: Doesn't work - no icon appears
        linkType = "other-unused", // TODO: Load our own sprite
        name = trapName,
      };

      // TODO: From HeartMonitor.Show(): local function InstantiateGuest(GuestData gd).
      //       Pull this out using a reverse transpiler patch instead of duplicating its logic.
      // TODO: Fix Pulse localization

      GameObject guestCreditObject = UnityEngine.Object.Instantiate(
        __instance.guestPrefab,
        __instance.creditsContainer
      );
      HeartMonitorGuest heartMonitorGuest = guestCreditObject.GetComponent<HeartMonitorGuest>();
      heartMonitorGuest.Setup(guestData);
      __instance.creditStrings.Add(guestData.name);
      __instance.links.Add(guestData.link);
      __instance.creditsElements.Add(guestCreditObject);

      // FIXME: Have to set name manually to mitigate localization
      //        This should instead be handled by Pulse
      heartMonitorGuest.nameText.text = trapName;
    }
  }

  // PreviewEnd is managed by TrapManager using an event from Pulse.

  [HarmonyPatch(typeof(scnLevelSelect), nameof(scnLevelSelect.GoToLevelSequence))]
  [HarmonyPostfix]
  private static void ApplyApplicableTrapsPatch(string levelToGo, scnLevelSelect __instance)
  {
    Plugin.Logger.LogInfo($"[{nameof(ArchipelagoModifierManagerPatch)}] Promoting preview traps to active traps");

    // Rhythm Dogtor and Rhythm Weightlifter dog mode can bypass selection,
    // so we may need apply preview patches here.
    if (levelToGo is nameof(Level.Lesmis) or nameof(Level.RhythmWeightlifter))
    {
      Plugin.Logger.LogInfo(
        $"[{nameof(ArchipelagoModifierManagerPatch)}] Going to Rhythm Dogtor/Rhythm Weightlifter (dog) - applying preview traps now"
      );
      // The cheat code for Rhythm Dogtor allows you to end it
      // hovering over any level, we need to check the levelToGo.
      Level level = RDUtils.ParseEnum(levelToGo, Level.None);
      // ReSharper disable once NullableWarningSuppressionIsUsed
      Plugin.StoryClient.ModifierManagerComponent!.PushAndApplyCompatibleTrapsInQueue(level);
    }

    // ReSharper disable once NullableWarningSuppressionIsUsed
    Plugin.StoryClient.ModifierManagerComponent!.PromotePreviewModifiers();
  }

  [HarmonyPatch(typeof(scnBase), nameof(scnBase.GoToLevel))]
  [HarmonyPostfix]
  private static void ApplyApplicableTrapsOnLevelChangePatch(string path)
  {
    // ActiveEnd should be invoked by ClearLocationPatch prior to this patch being invoked.
    Level level = RDUtils.ParseEnum(path, Level.None);

    if (level == Level.Montage2)
    {
      Plugin.Logger.LogWarning($"[{nameof(ArchipelagoModifierManagerPatch)}] Applying traps for {level} immediately");
      Plugin.StoryClient.ModifierManagerComponent!.PushAndApplyCompatibleTrapsInQueue(level);
      Plugin.StoryClient.ModifierManagerComponent!.PromotePreviewModifiers();
    }
  }

  /// <remarks>Unactive by clearing a level is handled by ClearLocationPatch.</remarks>
  [HarmonyPatch(typeof(scnGame), nameof(scnGame.Quit))]
  [HarmonyPostfix]
  private static void RestoreActiveTrapsOnAbandonPatch()
  {
    Plugin.Logger.LogInfo($"[{nameof(ArchipelagoModifierManagerPatch)}] Clearing active traps (returning to queue)");
    Plugin.StoryClient.ModifierManagerComponent!.ReturnActiveTrapsToQueue();
  }
}
