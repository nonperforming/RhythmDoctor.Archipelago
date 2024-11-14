#if DEBUG
namespace RhythmDoctor.Archipelago;

public class DebugMenu : MonoBehaviour
{
  private bool _activatedMain = false;
  private bool _activatedData = false;
  private bool _activatedMenu = false;

  private void Start()
  {
    Plugin.Logger.LogInfo("Debug menu started");
  }

  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.F3))
    {
      Plugin.Logger.LogInfo("Toggled Main Debug menu to " + !_activatedMain);
      _activatedMain = !_activatedMain;
    }
    if (Input.GetKeyDown(KeyCode.F4))
    {
      Plugin.Logger.LogInfo("Toggled Data Debug menu to " + !_activatedData);
      _activatedData = !_activatedData;
    }
    if (Input.GetKeyDown(KeyCode.F5))
    {
      Plugin.Logger.LogInfo("Toggled Menu Debug menu to " + !_activatedMenu);
      _activatedMenu = !_activatedMenu;
    }
  }

  private void OnGUI()
  {
    if (_activatedMain)
    {
      GUI.Box(new Rect(10, 10, 320, 60), "Rhythm Doctor Archipelago Main Debug");

      if (GUI.Button(new Rect(30, 30, 300, 20), "Toggle RD Debug"))
      {
        Plugin.Logger.LogInfo("Toggling RD Debug to " + !DebugSettings.instance.Debug);
        DebugSettings.instance.Debug = !DebugSettings.instance.Debug;
      }
    }

    if (_activatedData)
    {
      GUI.Box(new Rect(10, 10, 320, 150), "Rhythm Doctor Archipelago Data Debug");

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
      GUI.Box(new Rect(10, 10, 320, 60), "Rhythm Doctor Archipelago Menu Debug");

      if (GUI.Button(new Rect(30, 30, 300, 20), "Create Archipelago Menu Item"))
      {
        Plugin.Logger.LogInfo("Creating Archipelago Menu Item");

        CustomLevelsWardUIHelper.CreateCustomTab(
          "Archipelago",
          10,
          CustomTabAction,
          AssetHelper.LoadSprite(new WardIcons(), "archipelago.png"), // TODO: enum the asset name??
          null
        );
      }
    }
  }

  private void CustomTabAction()
  {
    Plugin.Logger.LogInfo("Archipelago button pressed");
  }
}
#endif
