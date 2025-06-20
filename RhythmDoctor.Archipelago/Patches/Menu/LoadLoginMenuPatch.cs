namespace RhythmDoctor.Archipelago.Patches.Menu;

[HarmonyPatch(typeof(scnMenu))]
static class ArchipelagoMenuOptionPatch
{
  private const string ARCHIPELAGO_OBJECT_NAME = "archipelago";

  [HarmonyPatch(nameof(scnMenu.Awake))]
  [HarmonyPrefix]
  static void RenameMusicOption(scnMenu __instance)
  {
    GameObject labelObject = __instance.transform.Find("mainMenu/options/optionsContainer/music").gameObject;
    labelObject.name = ARCHIPELAGO_OBJECT_NAME; // Otherwise this option can be hidden on Steam Deck
    Text text = labelObject.GetComponent<Text>();
    text.text = "Archipelago";
  }

  [HarmonyPatch(nameof(scnMenu.SelectOption))]
  [HarmonyPrefix]
  static void HandleArchipelagoOptionSelected(ref bool __runOriginal, scnMenu __instance)
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

  [HarmonyPatch(nameof(scnMenu.Localize))]
  [HarmonyPostfix]
  static void HandleArchipelagoOptionLocalize(scnMenu __instance)
  {
    // TODO: Patch RDString.Get instead and actually localize
    __instance
      .transform.Find($"mainMenu/options/optionsContainer/{ARCHIPELAGO_OBJECT_NAME}")
      .gameObject.GetComponent<Text>()
      .text = "Archipelago";
  }
}
