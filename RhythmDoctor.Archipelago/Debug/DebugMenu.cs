#if DEBUG
namespace RhythmDoctor.Archipelago.Debug;

public class DebugMenu : MonoBehaviour
{
  private bool _activatedMain;
  private bool _activatedData;
  private bool _activatedMenu;
  private bool _activatedLevels;

  private string _url = "archipelago.gg";

  // ReSharper disable once NullableWarningSuppressionIsUsed
  private string _username = null!;

  // ReSharper disable once NullableWarningSuppressionIsUsed
  private string _password = null!;
  private bool _deathLink;

  private void Start()
  {
    Plugin.Logger.LogInfo("Debug menu started");
  }

  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.F3))
    {
      _activatedMain = !_activatedMain;
      Plugin.Logger.LogDebug("Toggled Main Debug menu to " + _activatedMain);
    }
    if (Input.GetKeyDown(KeyCode.F4))
    {
      _activatedData = !_activatedData;
      Plugin.Logger.LogDebug("Toggled Data Debug menu to " + _activatedData);
    }
    if (Input.GetKeyDown(KeyCode.F5))
    {
      _activatedMenu = !_activatedMenu;
      Plugin.Logger.LogDebug("Toggled Menu Debug menu to " + _activatedMenu);
    }
    if (Input.GetKeyDown(KeyCode.F6))
    {
      _activatedLevels = !_activatedLevels;
      Plugin.Logger.LogDebug("Toggled Levels Debug menu to " + _activatedLevels);
    }
  }

  private void OnGUI()
  {
    if (_activatedMain)
    {
      GUI.Box(new Rect(10, 10, 330, 160), "Rhythm Doctor Archipelago Main Debug");

      if (GUI.Button(new Rect(30, 30, 300, 20), "Toggle RD Debug"))
      {
        Plugin.Logger.LogInfo("Toggling RD Debug to " + !DebugSettings.instance.Debug);
        DebugSettings.instance.Debug = !DebugSettings.instance.Debug;
      }

      GUI.Label(new Rect(30, 50, 150, 20), "URL");
      _url = GUI.TextField(new Rect(180, 50, 150, 20), _url);

      GUI.Label(new Rect(30, 70, 150, 20), "Username");
      _username = GUI.TextField(new Rect(180, 70, 150, 20), _username);

      GUI.Label(new Rect(30, 90, 150, 20), "Password");
      _password = GUI.TextField(new Rect(180, 90, 150, 20), _password);

      GUI.Label(new Rect(30, 110, 150, 20), "Death Link");
      _deathLink = GUI.Toggle(new Rect(180, 110, 150, 20), _deathLink, "");

      if (GUI.Button(new Rect(30, 130, 300, 20), "Connect"))
      {
        Plugin.Client = new Client.Client(_url, _username, _password);
      }
    }

    if (_activatedData)
    {
      GUI.Box(new Rect(10, 10, 330, 150), "Rhythm Doctor Archipelago Data Debug");

      ISerializer serializer = new SerializerBuilder()
        .WithNamingConvention(HyphenatedNamingConvention.Instance)
        .Build();

      if (GUI.Button(new Rect(30, 30, 300, 20), "Create ItemsData"))
      {
        Plugin.Logger.LogInfo("Creating ItemsData");
        ItemsData itemsData = DataHelper.GetItemsData();
        Plugin.Logger.LogInfo(serializer.Serialize(itemsData));
      }
      if (GUI.Button(new Rect(30, 60, 300, 20), "Create LocationsData"))
      {
        Plugin.Logger.LogInfo("Creating LocationsData");
        LocationsData locationsData = DataHelper.GetLocationsData();
        Plugin.Logger.LogInfo(serializer.Serialize(locationsData));
      }
      if (GUI.Button(new Rect(30, 90, 300, 20), "Create OptionsData"))
      {
        Plugin.Logger.LogInfo("Creating OptionsData");
        throw new NotImplementedException();
        //OptionsData optionsData = DataHelper.GetOptionsData();
        //Plugin.Logger.LogInfo(serializer.Serialize(optionsData));
      }
      if (GUI.Button(new Rect(30, 120, 300, 20), "Create WorldData"))
      {
        Plugin.Logger.LogInfo("Creating WorldData");
        throw new NotImplementedException();
        //WorldData worldData = DataHelper.GetWorldData();
        //Plugin.Logger.LogInfo(serializer.Serialize(worldData));
      }
    }

    if (_activatedMenu)
    {
      GUI.Box(new Rect(10, 10, 330, 60), "Rhythm Doctor Archipelago Menu Debug");

      if (GUI.Button(new Rect(30, 30, 300, 20), "Create Archipelago Menu Item"))
      {
        Plugin.Logger.LogInfo("Creating Archipelago Menu Item");

        CustomLevelsWardHelper.CreateCustomTab(
          "Archipelago",
          10,
          CustomTabAction,
          AssetHelper.LoadSprite(new WardIcons(), "archipelago.png"), // TODO: enum the asset name??
          null
        );
      }
    }

    if (_activatedLevels)
    {
      GUI.Box(new Rect(10, 10, 330, 200), "Rhythm Doctor Archipelago Levels Debug");

      if (GUI.Button(new Rect(30, 30, 300, 20), "Lock all"))
      {
        foreach (Level level in Enum.GetValues(typeof(Level)))
        {
          Persistence.SetLevelRank(level, Rank.NotAvailable, force: true);
        }
        scnBase.GoToScene("scnLevelSelect");
      }

      if (GUI.Button(new Rect(30, 60, 300, 20), "Unlock all"))
      {
        foreach (Level level in Enum.GetValues(typeof(Level)))
        {
          Persistence.SetLevelRank(level, Rank.NotFinished, force: true);
        }
        scnBase.GoToLevelSelect();
      }

      if (GUI.Button(new Rect(30, 90, 300, 20), "Unlock all entrances"))
      {
        scnLevelSelect.instance.UnlockEntrance(scnLevelSelect.instance.FindSelectableEntity("GoToSVTWard"));
        scnLevelSelect.instance.UnlockEntrance(scnLevelSelect.instance.FindSelectableEntity("GoToTrain"));
        scnLevelSelect.instance.UnlockEntrance(scnLevelSelect.instance.FindSelectableEntity("GoToBasement"));
        scnLevelSelect.instance.UnlockEntrance(scnLevelSelect.instance.FindSelectableEntity("GoToAthleteWard"));
      }

      if (GUI.Button(new Rect(30, 120, 300, 20), "Unlock level 3-1"))
      {
        Persistence.SetLevelRank(Level.Garden, Rank.NotFinished);
        scnBase.GoToLevelSelect();
      }

      if (GUI.Button(new Rect(30, 150, 300, 20), "Unlock level 3-2N"))
      {
        Persistence.SetLevelRank(Level.Lounge, Rank.NotFinished);
        scnBase.GoToLevelSelect();
      }

      if (GUI.Button(new Rect(30, 180, 300, 20), "S+ selected level"))
      {
        if (scnLevelSelect.instance.selectedEntity is not SelectableCharacter selectableCharacter)
          return;

        Level selectedLevel = selectableCharacter.levels[scnLevelSelect.instance.currentDifficulty];
        Persistence.SetLevelRank(selectedLevel, Rank.Splus, force: true);
        Persistence.SetLastPlayedLevel(selectedLevel);
        scnBase.GoToLevelSelect();
      }
    }
  }

  private void CustomTabAction()
  {
    Plugin.Logger.LogInfo("Archipelago button pressed");
  }
}
#endif
