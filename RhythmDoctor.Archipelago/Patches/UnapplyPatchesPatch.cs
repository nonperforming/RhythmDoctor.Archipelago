namespace RhythmDoctor.Archipelago.Patches;

[HarmonyPatch(typeof(scnBase))]
public class UnapplyPatchesPatch
{
  [HarmonyPatch(nameof(scnBase.GoToMainMenu))]
  [HarmonyPrefix]
  public static void GoToMainMenu()
  {
    // TODO: Tear down Archipelago before applying menu patches
    Plugin.UnapplyGameplayPatches();
  }
}
