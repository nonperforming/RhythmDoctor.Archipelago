namespace RhythmDoctor.Archipelago.Patches.Menu;

[HarmonyPatch(typeof(scnCLS))]
internal static class ArchipelagoLoginPatch
{
  // FIXME: This isn't matching for some reason!
  // [HarmonyPatch(typeof(scnCLS), nameof(scnCLS.Awake))]
  // [HarmonyTranspiler]
  // private static IEnumerable<CodeInstruction> ShowAllWardOptions(IEnumerable<CodeInstruction> instructions)
  // {
  //   // Steam ward option (if Steam isn't initialized) and Import ward option (if on the Steam Deck)
  //   return new CodeMatcher()
  //     // if (!SteamIntegration.initialized)
  //     .MatchForward(false, new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(SteamIntegration), nameof(SteamIntegration.initialized))))
  //     .SetOpcodeAndAdvance(OpCodes.Nop) // ldsfld bool SteamIntegration::initialized
  //     .SetOpcodeAndAdvance(OpCodes.Nop) // brtrue.s IL_01d3
  //     // if (Persistence.GetFeatureSet() == FeatureSet.SteamDeck)
  //     .MatchForward(false, new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Persistence), nameof(Persistence.GetFeatureSet))))
  //     .SetOpcodeAndAdvance(OpCodes.Nop) // call valuetype FeatureSet Persistence::GetFeatureSet()
  //     .SetOpcodeAndAdvance(OpCodes.Nop) // ldc.i4.1
  //     .SetOpcodeAndAdvance(OpCodes.Nop) // bne.un.s IL_022e
  //     .InstructionEnumeration();
  // }

  [HarmonyPatch(nameof(scnCLS.Start))]
  [HarmonyPostfix]
  private static void ConstructArchipelagoMenu(scnCLS __instance)
  {
    Plugin.Logger.LogInfo("Renaming custom level ward items");

    #region Rename LED sign
    Plugin.Logger.LogInfo("Renaming LED sign");
    GameObject.Find("Canvas/Ward Container/LEDSign Container/WardTitle Text").GetComponent<Text>().text = "ARCHIPELAGO";
    #endregion

    #region Rename ward options
    Plugin.Logger.LogInfo("Renaming ward options");
    // Get WardOptions
    scnCLS.WardOption libraryOption = __instance.wardOptions.Find(wardOption =>
      wardOption.name == scnCLS.WardOptionName.Library
    );
    // TODO: ShowAllWardOptions!!!
    scnCLS.WardOption? workshopOption = __instance.wardOptions.Find(wardOption =>
      wardOption.name == scnCLS.WardOptionName.OpenSteamWorkshop
    );
    scnCLS.WardOption? importOption = __instance.wardOptions.Find(wardOption =>
      wardOption.name == scnCLS.WardOptionName.ImportLevels
    );

    // Delete Library and Steam Workshop options
    if (libraryOption != null)
    {
      libraryOption.rect?.transform.parent?.gameObject.SetActive(false);
      __instance.wardOptions.Remove(libraryOption);
    }

    if (workshopOption != null)
    {
      workshopOption.rect?.transform.parent?.gameObject.SetActive(false);
      __instance.wardOptions.Remove(workshopOption);
    }
    #endregion

    #region Import to Archipelago option
    Plugin.Logger.LogInfo("Renaming Import to Archipelago option");
    // WardOption.rect returns ImportSign Container.
    // ImportLevels/ImportSign Container/Button/Text
    Transform buttonObject = importOption.rect.Find("Button");
    buttonObject.Find("Icon Image").GetComponent<Image>().sprite = AssetHelper.LoadSprite(
      new WardIcons(),
      "archipelago.png"
    );
    buttonObject.Find("Text").GetComponent<Text>().text = "Archipelago";
    #endregion
  }

  [HarmonyPatch(typeof(LevelImporter), nameof(LevelImporter.Install))]
  [HarmonyPrefix]
  private static void OverrideInstallButton(ref bool __runOriginal, LevelImporter __instance)
  {
    // ReSharper disable once GrammarMistakeInComment
    // We let Install run up to the first yield
    // This prevents the user from changing the text, plays a sound, and changes the CurrentContentName to Installing.

    __runOriginal = false;

    string[] text = __instance
      .transform.Find("screen/Contents/InsertURL Container/URL InputField")
      .GetComponent<Text>()
      .text.Split('\n');

    string? url = text[0];
    string? name = text[1];
    string? password = null;
    try
    {
      password = text[2];
    }
    catch (IndexOutOfRangeException)
    { }

    if (url.IsNullOrWhiteSpace() || name.IsNullOrWhiteSpace())
    {
      // Invalid information
      return;
    }

    // Attempt to log in with the information given.
    try
    {
      Plugin.Client = new Client.Client(url, name, password);
    }
    catch
    {
      throw new NotImplementedException();
    }

    // Successful login
    __instance.cls.CLSPlaySound("sndImportInstallFinish");
  }

  [HarmonyPatch(nameof(scnCLS.Exit))]
  [HarmonyPostfix]
  private static void UnpatchMenu()
  {
    Plugin.UnapplyArchipelagoMenuPatch();
  }

  [HarmonyPatch(nameof(scnCLS.SelectWardOption))]
  [HarmonyPostfix]
  private static void CustomSelectOption(ref bool __runOriginal, scnCLS __instance)
  {
    __runOriginal = false;
    switch (__instance.CurrentWardOption.name)
    {
      case scnCLS.WardOptionName.Library:
      case scnCLS.WardOptionName.OpenSteamWorkshop:
        // It should not be possible to select these, but just in case.
        Plugin.Logger.LogWarning("Library/Steam Workshop selected in Archipelago login screen");
        return;
      default:
        // Exit or import options
        __runOriginal = true;
        break;
    }
  }

  [HarmonyPatch(typeof(LevelImporter), nameof(LevelImporter.Showing), MethodType.Setter)]
  [HarmonyPostfix]
  private static void ArchipelagoImportScreen()
  {
    GameObject levelImporterObject = scnCLS.instance.levelImporter.gameObject;
    GameObject screenObject = levelImporterObject.transform.Find("screen").gameObject;
    GameObject contentsObject = screenObject.transform.Find("Contents").gameObject;

    GameObject urlContainerObject = contentsObject.transform.Find("InsertURL Container").gameObject;
    GameObject addButtonObject = urlContainerObject.transform.Find("Add Button").gameObject;
    GameObject urlInputFieldContainerObject = urlContainerObject.transform.Find("URL InputField").gameObject;
    GameObject placeholderObject = urlInputFieldContainerObject.transform.Find("Placeholder").gameObject;
    Text placeholderText = placeholderObject.GetComponent<Text>();
    Text instructionsText = urlContainerObject.transform.Find("Instructions").GetComponent<Text>();
    Text addButtonText = addButtonObject.transform.Find("Text").GetComponent<Text>();

    // Open the "INSTALL LEVELS" screen
    contentsObject.transform.Find("Draggable Content/AddURL Button").GetComponent<Button>().onClick.Invoke();

    // Rename the topbar from "INSTALL LEVELS" to "ARCHIPELAGO LOGIN"
    contentsObject.transform.Find("Top Panel/Title Text").GetComponent<Text>().text = "ARCHIPELAGO LOGIN";

    urlContainerObject.transform.Find("Cancel Button").gameObject.SetActive(false);

    instructionsText.text = "Put in your client information in the format given and hit Connect.";
    placeholderText.text = "<URL>\n<Name>\n<Password>";
    addButtonText.text = "Connect";
  }
}
