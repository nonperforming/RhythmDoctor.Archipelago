namespace RhythmDoctor.Archipelago.Patches;

/// <summary>
/// Force 1-CNY and 1-BOO to be available regardless of date
/// </summary>
internal static class ForceLevelAvailablePatch
{
  // FIXME: Patching these two methods could lead to side effects
  //        Is there a way we can directly enable the levels instead?
  // FIXME: Currently 1-CNY and 1-BOO take up the same space on level select.

  [HarmonyPatch(typeof(scnLevelSelect), nameof(scnLevelSelect.CheckCNY))]
  [HarmonyPrefix]
  static void ForceCNYPatch(ref bool __runOriginal, ref bool __result)
  {
    Plugin.Logger?.LogDebug("Forcing CNY check");
    __result = true;
    __runOriginal = false;
  }

  [HarmonyPatch(typeof(RDBase), nameof(RDBase.IsHalloweenWeek))]
  [HarmonyPrefix]
  static void ForceHalloweenPatch(ref bool __runOriginal, ref bool __result)
  {
    Plugin.Logger?.LogDebug("Forcing Halloween Week check");
    __result = true;
    __runOriginal = false;
  }
}
