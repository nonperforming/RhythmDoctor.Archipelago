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


#if DEBUG
  private void Start()
  {
    StartCoroutine(nameof(CreateDebugMenu), 5.0f);
  }

  private IEnumerator CreateDebugMenu()
  {
    Logger.LogInfo("Creating debug menu");
    GameObject debugMenu = new("Archipelago Debug Menu");
    DontDestroyOnLoad(debugMenu);
    debugMenu.AddComponent<DebugMenu>();

    yield return null;
  }
#endif
}
