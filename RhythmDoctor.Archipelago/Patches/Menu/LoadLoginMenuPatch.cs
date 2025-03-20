namespace RhythmDoctor.Archipelago.Patches.Menu;

[HarmonyPatch(typeof(scnMenu))]
internal static class ArchipelagoMenuOptionPatch
{
  private const string ARCHIPELAGO_OBJECT_NAME = "archipelago";

  [HarmonyPatch(nameof(scnMenu.Awake))]
  [HarmonyPrefix]
  internal static void RenameMusicOption(ref scnMenu __instance)
  {
    GameObject labelObject = __instance.transform.Find("mainMenu/options/optionsContainer/music").gameObject;
    labelObject.name = ARCHIPELAGO_OBJECT_NAME; // Otherwise this option can be hidden on Steam Deck
    Text text = labelObject.GetComponent<Text>();
    text.text = "Archipelago";
  }

  [HarmonyPatch(nameof(scnMenu.SelectOption))]
  [HarmonyPrefix]
  internal static void HandleArchipelagoOptionSelected(ref bool __runOriginal, ref scnMenu __instance)
  {
    bool archipelagoOptionSelected =
      __instance.optionsText[__instance.currentOption].gameObject.name == ARCHIPELAGO_OBJECT_NAME;

    if (archipelagoOptionSelected)
    {
      __runOriginal = false;
      Plugin.ApplyArchipelagoMenuPatch();
      __instance.PlayConfirmSound();
      __instance.TransitionToScene("scnCLS");
    }
  }
}
