namespace RhythmDoctor.Archipelago.Patches;

/// <summary>
/// Force all Janitors to be visible.
/// </summary>
[HarmonyPatch(typeof(scnLevelSelect))]
internal static class ShowAllJanitorsPatch
{
  [HarmonyPatch(nameof(scnLevelSelect.HideJanitor))]
  [HarmonyPrefix]
  static void Prefix(ref bool __runOriginal)
  {
    Plugin.Logger?.LogDebug("Bypassing HideJanitor");
    __runOriginal = false;
  }
}
