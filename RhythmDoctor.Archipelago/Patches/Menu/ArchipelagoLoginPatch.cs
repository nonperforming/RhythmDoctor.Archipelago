namespace RhythmDoctor.Archipelago.Patches.Menu;

// TODO: This script needs a cleanup, maybe don't use scnCLS as base for login?

[HarmonyPatch]
internal static class ArchipelagoLoginPatch
{
  [HarmonyPatch(typeof(scnCLS), nameof(scnCLS.Start))]
  [HarmonyPostfix]
  private static void ConstructArchipelagoMenuPatch(scnCLS __instance)
  {
    // TODO: Need to change applicable text on LevelDetail

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

    // We need to deselect the Library option first otherwise we get weird issues with UI
    __instance.ChangeToImportOption();

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
      AssetHelper.AssetType.WardIcons.TYPE,
      AssetHelper.AssetType.WardIcons.ARCHIPELAGO
    );
    buttonObject.Find("Text").GetComponent<Text>().text = "Archipelago";
    #endregion

    #region Hiding other Install Levels options
    //Plugin.Logger.LogInfo("Hiding other install levels options");
    //GameObject draggableContent = GameObject.Find("/Canvas/LevelImporter/screen/Contents/Draggable Content");
    //draggableContent.transform.GetChild(0).gameObject.SetActive(false); // notDraggableInstructions
    //draggableContent.transform.GetChild(1).gameObject.SetActive(false); // draggableInstructions
    //draggableContent.transform.GetChild(2).gameObject.SetActive(false); // Browse Button
    //draggableContent.transform.GetChild(3).gameObject.SetActive(true); // AddURL Button
    #endregion

    Plugin.Logger.LogInfo("Disabling DragAndDrop");
    __instance.levelImporter.dragAndDrop.SetActive(false);

    // Fix UI breaking after changing selection
    __instance._currentWardOptionIndex = 0;
  }

  [HarmonyPatch(typeof(LevelImporter), nameof(LevelImporter.Install))]
  [HarmonyPostfix]
  private static IEnumerator OverrideInstallButtonPatch(IEnumerator result, LevelImporter __instance)
  {
    // TODO: Show appropriate errors rather than silently failing

    #region Lock input
    Plugin.Logger.LogInfo("Locking input");
    __instance.cls.CLSPlaySound("sndImportInstallButtonClick");
    __instance.ToggleInsertUrlContainer(false);
    __instance.CurrentContentName = LevelImporter.ContentName.Installing;
    __instance.stoppedInstallCoroutine = false;
    __instance.CanToggleClearButton = false;
    __instance.levelsToInstall = new List<ImportLevel>(__instance.toInstallIS.levels);
    foreach (ImportLevel item in __instance.levelsToInstall)
    {
      item.canToggleRemoveButton = false;
    }
    yield return null;
    #endregion

    string[] text = __instance
      .transform.Find("screen/Contents/InsertURL Container/URL InputField/Text")
      .GetComponent<Text>()
      .text.Split('\n');
    Plugin.Logger.LogInfo($"Input: {text.Join()}");

    if (text.Length < 2)
    {
      Plugin.Logger.LogError("URL or Name not input, bailing out");
      // URL or Name are not present, bail out
      __instance.CurrentContentName = LevelImporter.ContentName.LevelsInstalled;
      __instance.cls.CLSPlaySound("sndImportInstallFinish");
      __instance.AddLevelToErrorSection(__instance.levelsToInstall[0], "URL or Name not present");
      yield break;
    }

    string url = text[0];
    string name = text[1];
    string? password = null;
    try
    {
      password = text[2];
    }
    catch (IndexOutOfRangeException)
    {
      // No password given.
    }
    Plugin.Logger.LogInfo($"URL: {url}, Slot Name: {name}");

    if (url.IsNullOrWhiteSpace() || name.IsNullOrWhiteSpace())
    {
      // Invalid information, bail out
      Plugin.Logger.LogError("Invalid information, bailing out");
      __instance.CurrentContentName = LevelImporter.ContentName.LevelsInstalled;
      __instance.cls.CLSPlaySound("sndImportInstallFinish");
      __instance.AddLevelToErrorSection(__instance.levelsToInstall[0], "URL or Name not present"); // FIXME: doesn't show
      __instance.transform.Find("screen/Contents/Top Panel/Title Text").GetComponent<Text>().text = "LOGIN FAILED";
      yield break;
    }

    // Attempt to log in with the information given.
    Plugin.Logger.LogInfo("Creating client");
    try
    {
      // TODO: Make async, do not hang
      Plugin.Client = new Client.Client(url, name, password);
    }
    catch (Exception exception)
    {
      Plugin.Logger.LogError(exception);
      try
      {
        Plugin.Client.Dispose();
      }
      catch
      {
        // ignore all exceptions
      }
      Plugin.Client = null!;
      // Bail out
      __instance.CurrentContentName = LevelImporter.ContentName.LevelsInstalled;
      __instance.cls.CLSPlaySound("sndImportInstallFinish");
      __instance.AddLevelToErrorSection(__instance.levelsToInstall[0], exception.Message); // FIXME: Not showing anything
      __instance.transform.Find("screen/Contents/Top Panel/Title Text").GetComponent<Text>().text = "LOGIN FAILED";
      yield break;
    }

    // Successful login
    Plugin.Logger.LogInfo("Logged in!");
    __instance.cls.CLSPlaySound("sndImportInstallFinish");
    yield return null;
    Persistence.currentSlotIndex = 0; // Slot 1
    Plugin.ApplyGameplayPatches();

    // Scary!!!!!!!!!!!
    // Hopefully if we got here without any exceptions SavingPatch should be applied,
    //  so we shouldn't lose our first slot in the case of a crash.
    // When we are quitting, the original data should be reloaded by UnapplyPatchesPatch.
    Persistence.slotPrefs[0].dict.Clear();

    // Let LockSleevePaintPatch set the Sleeve Paint to Slot 1's default
    Persistence.p1Skin.Reload();
    Persistence.p2Skin.Reload();

    // Some levels come unlocked by default, such as X-1.
    // Lock all levels to force the user to unlock them with an item.
    foreach (Level level in Enum.GetValues(typeof(Level)))
    {
      Persistence.SetLevelRank(level, Rank.NotAvailable, true);
    }

    // Setup DataStorage and TrapManager.ClearedTraps, initial Paige stays state (this can change!)
    Plugin
      .Client.Session!.DataStorage[Scope.Slot, Persistence.PaigeStaysKey]
      .Initialize(Plugin.Random.Next() % 2 == 1);
    Persistence.SetPaigeEnding(Plugin.Client.Session!.DataStorage[Scope.Slot, Persistence.PaigeStaysKey].To<bool>());
    foreach (Type trapType in Bindings.Traps)
    {
      ITrap trap = (ITrap)Activator.CreateInstance(trapType);
      // ReSharper disable once NullableWarningSuppressionIsUsed
      Plugin.Client.Session!.DataStorage[Scope.Slot, trap.Name].Initialize(0);
      Plugin.Client.TrapManager.ClearedTraps.Add(trap.Name, 0);
    }
    Plugin.Client.ReadyForItems = true;
    UnpatchMenuPatch();

    Plugin.Logger.LogInfo("Heading to Level Select...");
    scnBase.GoToScene("scnLevelSelect");
  }

  [HarmonyPatch(typeof(scnCLS), nameof(scnCLS.Exit))]
  [HarmonyPostfix]
  private static void UnpatchMenuPatch()
  {
    Plugin.UnapplyArchipelagoMenuPatch();
  }

  [HarmonyPatch(typeof(scnCLS), nameof(scnCLS.SelectWardOption))]
  [HarmonyPostfix]
  private static void CustomSelectOptionPatch(ref bool __runOriginal, scnCLS __instance)
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
  private static void ArchipelagoImportScreenPatch()
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

    // Rename the top bar from "INSTALL LEVELS" to "ARCHIPELAGO LOGIN"
    contentsObject.transform.Find("Top Panel/Title Text").GetComponent<Text>().text = "ARCHIPELAGO LOGIN";

    // FIXME: This removes the button to go back a page but the user can still use the 'escape' keybind
    urlContainerObject.transform.Find("Cancel Button").gameObject.SetActive(false);

    instructionsText.text = "Put in your client information in the format given and hit Connect.";
    placeholderText.text = "<URL>\n<Name>\n<Password>";
    addButtonText.text = "Connect";
  }

  [HarmonyPatch(typeof(LevelImporter), nameof(LevelImporter.ValidateUrl))]
  [HarmonyPrefix]
  private static void StubValidateUrlPatch(ref bool __runOriginal, LevelImporter __instance)
  {
    // Seems to be called on the "install" button being clicked.
    // Redirect it to our login patch.
    __runOriginal = false;
    __instance.Install_Public();
  }

  [HarmonyPatch(typeof(LevelImporter), nameof(LevelImporter.CanDragAndDrop), MethodType.Getter)]
  [HarmonyPrefix]
  private static void DisableDragAndDropInstallGetPatch(ref bool __result, ref bool __runOriginal)
  {
    __runOriginal = false;
    __result = false;
  }

  [HarmonyPatch(typeof(LevelImporter), nameof(LevelImporter.CanDragAndDrop), MethodType.Setter)]
  [HarmonyPrefix]
  private static void DisableDragAndDropInstallSetPatch(ref bool __runOriginal)
  {
    __runOriginal = false;
  }
}
