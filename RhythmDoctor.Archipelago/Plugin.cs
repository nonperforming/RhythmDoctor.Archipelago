using BepInEx;
using BepInEx.Logging;

namespace RhythmDoctor.Archipelago;

[BepInPlugin(PluginInformation.GUID, PluginInformation.Name, PluginInformation.Version)]
[BepInProcess("Rhythm Doctor.exe")]
public class Plugin : BaseUnityPlugin
{
  internal static new ManualLogSource Logger;

  private void Awake()
  {
    Logger = base.Logger;
    Logger.LogInfo($"Plugin {PluginInformation.GUID} version {PluginInformation.Version} is loaded!");
  }
}
