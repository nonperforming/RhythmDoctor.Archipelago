namespace RhythmDoctor.Archipelago.Patches;

/// <summary>
/// Disable all saving progress to disk, and load Sleeve Paint images from our special AP file.
/// </summary>
[HarmonyPatch(typeof(Persistence))]
internal static class SavingPatch
{
  [HarmonyPatch(nameof(Persistence.SaveSlot))]
  [HarmonyPrefix]
  private static void DisableSavingToFilePatch(int slot, ref bool __runOriginal)
  {
    // "Slot" -1 is settings, as shown by GetSavefilePath
    // string text = ((slot == -1) ? "settings.rdsave" : $"slot{slot}.rdsave");
    __runOriginal = slot == -1;
  }

  [HarmonyPatch(typeof(ArmSkin), nameof(ArmSkin.GetDrawingPath))]
  [HarmonyPrefix]
  private static void RedirectSleevePaintFilePatch(RDPlayer player, ref string __result, ref bool __runOriginal)
  {
    __runOriginal = false;
    // .ToString() shouldn't modify an enum???
#pragma warning disable Harmony003
    __result = Path.Combine(Persistence.GetSaveFileFolderPath(), "scribble", $"{player.ToString()}_AP.png");
#pragma warning restore Harmony003
  }
}
