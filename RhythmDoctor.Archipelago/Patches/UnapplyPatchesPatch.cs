namespace RhythmDoctor.Archipelago.Patches;

[HarmonyPatch]
internal static class UnapplyPatchesPatch
{
  // Access modifier is internal instead of private as the finalizer for Plugin can call it
  // (i.e. when running under ScriptEngine).
  [HarmonyPatch(typeof(scnMenu), nameof(scnMenu.Awake))] // Just in case!
  [HarmonyPatch(typeof(scnBase), nameof(scnBase.GoToMainMenu))]
  [HarmonyPrefix]
  internal static void TearDownClientPluginPatch()
  {
    Plugin.Logger.LogInfo("Tearing down client plugin");
    Plugin.UnapplyGameplayPatches();

    // Reload data - we wipe Slot 1 in ArchipelagoLoginPatch, and we do **NOT** want to lose it.
    Persistence.Load();
    // In the case we somehow skip scnBase.GoToMainMenu (maybe some other plugin) we need to reload slot 1's data,
    //  as otherwise the Main Menu option will still show our Archipelago slot.
    try
    {
      scnMenu.instance.slots[0].LoadSlotData();
      Plugin.Logger.LogDebug("Reloaded slot data");
    }
    catch (NullReferenceException)
    {
      // We aren't in the Main Menu yet. Don't do anything.
      Plugin.Logger.LogDebug("Not in main menu, could not load slot data");
    }

    // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
    Plugin.Client?.Dispose();
    // ReSharper disable once NullableWarningSuppressionIsUsed
    Plugin.Client = null!;
  }
}
