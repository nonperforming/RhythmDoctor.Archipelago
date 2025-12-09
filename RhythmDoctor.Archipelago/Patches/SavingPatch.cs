namespace RhythmDoctor.Archipelago.Patches;

/// <summary>
/// Disable all saving progress to disk, and load Sleeve Paint images from our special AP file.
/// </summary>
[HarmonyPatch]
internal static class SavingPatch
{
  [HarmonyPatch(typeof(PlayerPrefsJson), nameof(PlayerPrefsJson.Save))]
  [HarmonyPatch(typeof(PlayerPrefsJson), nameof(PlayerPrefsJson.SaveBackup))]
  [HarmonyPrefix]
  private static void DisableSavingToFilePatch(int slot, ref bool __runOriginal, ref PlayerPrefsJson __instance)
  {
    // Let the user change and save settings.
    __runOriginal = __instance.fileType == PlayerPrefsJson.FileType.Settings;
  }

  [HarmonyPatch(typeof(ArmSkin), nameof(ArmSkin.GetDrawingPath))]
  [HarmonyPrefix]
  private static void RedirectSleevePaintFilePatch(RDPlayer player, ref string __result, ref bool __runOriginal)
  {
    __runOriginal = false;
    // .ToString() shouldn't modify an enum???
#pragma warning disable Harmony003
    __result = Path.Combine(PlayerPrefsJson.GetFileFolderPath(), $"scribble{player.ToString()}_AP.png");
#pragma warning restore Harmony003
  }
}
