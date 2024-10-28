using System.Reflection;

namespace RhythmDoctor.Archipelago;

/// <summary>
/// Apply all patches and create the Logger
/// </summary>
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Rhythm Doctor.exe")]
public class Plugin : BaseUnityPlugin
{
  internal static Client.Client? client;
  internal static new ManualLogSource Logger = null!;

  private static readonly Type[] Patches =
  [
    //typeof(CustomLevelsWardUIPatch),
    typeof(ForceCNYAvailablePatch),
    //typeof(JanitorPatch),
    typeof(LevelSelectPatch),
    typeof(VersionTextPatch),
  ];

  /// <summary>
  /// Apply patches
  /// </summary>
  private void Awake()
  {
    Logger = base.Logger;
    Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} loaded");

    Logger.LogInfo("Applying patches");
    foreach (Type patch in Patches)
    {
      Logger.LogDebug($"Applying {patch.Name}");
      Harmony.CreateAndPatchAll(patch);
    }
  }
}
