namespace RhythmDoctor.Archipelago.Patches;

/// <summary>
/// Force 1-CNY to be available regardless of date
/// </summary>
[HarmonyPatch(typeof(scnLevelSelect))]
internal static class ForceCNYAvailablePatch
{
  [HarmonyPatch(nameof(scnLevelSelect.CheckCNY))]
  [HarmonyPrefix]
  static void ForceCNYPatch(ref bool __runOriginal, ref bool __result)
  {
    Plugin.Logger?.LogDebug("Forcing CNY check");
    __result = true;
    __runOriginal = false;
  }
}
