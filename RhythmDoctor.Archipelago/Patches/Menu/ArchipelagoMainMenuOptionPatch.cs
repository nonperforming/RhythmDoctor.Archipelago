namespace RhythmDoctor.Archipelago.Patches.Menu;

[HarmonyPatch(typeof(scnMenu))]
internal static class ArchipelagoMainMenuOptionPatch
{
  private const string ARCHIPELAGO_OBJECT_NAME = "archipelago";

  // TODO: We should create our own menu option instead of overriding one.
  [HarmonyPatch(nameof(scnMenu.Awake))]
  [HarmonyPrefix]
  private static void RenameMusicOptionPatch(scnMenu __instance)
  {
    GameObject labelObject = __instance.transform.Find("mainMenu/options/optionsContainer/music").gameObject;
    // We need to set the object name to something other than 'music'
    // otherwise this option can be hidden on Steam Deck
    labelObject.name = ARCHIPELAGO_OBJECT_NAME;
  }

  [HarmonyPatch(nameof(scnMenu.SelectOption))]
  [HarmonyPrefix]
  private static void HandleArchipelagoOptionSelectedPatch(ref bool __runOriginal, scnMenu __instance)
  {
    bool archipelagoOptionSelected =
      __instance.optionsText[__instance.currentOption].gameObject.name == ARCHIPELAGO_OBJECT_NAME;

    if (!archipelagoOptionSelected)
      return;

    __runOriginal = false;
    Plugin.ApplyArchipelagoMenuPatch();
    __instance.PlayConfirmSound();
    __instance.TransitionToScene("scnCLS");
  }
}
