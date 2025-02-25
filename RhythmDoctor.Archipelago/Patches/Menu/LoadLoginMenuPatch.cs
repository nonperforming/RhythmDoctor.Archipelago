namespace RhythmDoctor.Archipelago.Patches.Menu;

[HarmonyPatch(typeof(scnMenu))]
internal static class LoadLoginMenuPatch
{
  [HarmonyPatch(nameof(scnMenu.Start))]
  [HarmonyPostfix]
  internal static void StartPostfix(scnMenu __instance)
  {
    scnBase.GoToCustomLevelSelect();
  }
}
