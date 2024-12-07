namespace RhythmDoctor.Archipelago.Patches;

// TODO: Make this check for the relevant item!
[HarmonyPatch(typeof(RDUtils))]
internal static class UnlockLevelFromItemPatch
{
  [HarmonyPatch(nameof(RDUtils.Locked))]
  [HarmonyPrefix]
  static void Prefix(this Level level, ref bool __runOriginal, ref bool __result)
  {
    // TODO: This doesn't actually seem to do anything.
    Plugin.Logger?.LogDebug("Bypassing locked check");
    __runOriginal = false;
    __result = false;
  }
}
