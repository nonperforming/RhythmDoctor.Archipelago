namespace RhythmDoctor.Archipelago.Patches;

[HarmonyPatch(typeof(scnBase))]
static class UnapplyPatchesPatch
{
  [HarmonyPatch(nameof(scnBase.GoToMainMenu))]
  [HarmonyPrefix]
  static void GoToMainMenu()
  {
    // TODO: Tear down Archipelago before applying menu patches
    Plugin.UnapplyGameplayPatches();
  }
}
