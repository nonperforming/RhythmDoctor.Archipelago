#if DEBUG
namespace RhythmDoctor.Archipelago;

public class DebugMenu : MonoBehaviour
{
  private void OnGUI()
  {
    // Background
    GUI.Box(new Rect(10, 10, 320, 370), "Rhythm Doctor Archipelago Debug");

    ISerializer serializer = new SerializerBuilder()
      .WithNamingConvention(HyphenatedNamingConvention.Instance)
      .Build();

    if (GUI.Button(new Rect(30, 30, 300, 20), "Toggle RD Debug"))
    {
      DebugSettings.instance.Debug = !DebugSettings.instance.Debug;
    }

    if (GUI.Button(new Rect(30, 60, 300, 20), "Create ItemsData"))
    {
      ItemsData itemsData = DataFileHelper.GetItemsData();
      Plugin.Logger.LogInfo(serializer.Serialize(itemsData));
    }
    // if (GUI.Button(new Rect(30, 130, 300, 50), "Create LocationsData"))
    // {
    //   ItemsData locationsData = new LocationsData();
    //   Plugin.Logger.LogInfo(serializer.Serialize(locationsData));
    // }
    // if (GUI.Button(new Rect(30, 230, 300, 50), "Create OptionsData"))
    // {
    //   ItemsData optionsData = new OptionsData();
    //   Plugin.Logger.LogInfo(serializer.Serialize(optionsData));
    // }
    // if (GUI.Button(new Rect(30, 330, 300, 50), "Create OptionsData"))
    // {
    //   ItemsData worldOptions = new OptionsData();
    //   Plugin.Logger.LogInfo(serializer.Serialize(worldOptions));
    // }
  }
}
#endif
