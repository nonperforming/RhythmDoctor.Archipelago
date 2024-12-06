namespace RhythmDoctor.Archipelago.Patches;

/// <summary>
/// Force all Janitors to be visible.
/// </summary>
[HarmonyPatch(typeof(scnLevelSelect))]
internal static class ShowAllJanitorsPatch
{
  [HarmonyPatch(nameof(scnLevelSelect.PlaceJanitor))]
  [HarmonyPrefix]
  static void PlaceJanitorPatch(ref bool __runOriginal)
  {
    Plugin.Logger?.LogDebug("Bypassing PlaceJanitor");
    __runOriginal = false;
  }

  [HarmonyPatch(nameof(scnLevelSelect.HideJanitor))]
  [HarmonyPrefix]
  static void Prefix(ref bool __runOriginal)
  {
    Plugin.Logger?.LogWarning("Bypassing HideJanitor. This should never be called assuming PlaceJanitor was bypassed!");
    __runOriginal = false;
  }
}
