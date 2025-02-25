namespace RhythmDoctor.Archipelago;

/// <summary>
/// Apply all patches and create the Logger
/// </summary>
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Rhythm Doctor.exe")]
public class Plugin : BaseUnityPlugin
{
  internal static Client.Client Client = null!;
  internal static new ManualLogSource Logger = null!;

  private static readonly Type[] MenuPatches =
  [
    typeof(ArchipelagoLoginPatch),
    typeof(LoadLoginMenuPatch),
    typeof(VersionTextPatch),
  ];

  private static readonly Type[] GameplayPatches =
  [
    typeof(ArchipelagoMenuPatch),
    typeof(ClearLocationPatch),
    typeof(JanitorPatch),
    typeof(NicoleBlockagePatch),
    //typeof(SkipTutorialPatch), // Disabled due to bugs with 1-2 and 3-X. See class for more information
    //typeof(UnlockItemPatch),
#if DEBUG
    typeof(LogClearLevelPatch),
#endif
  ];

  /// <summary>
  /// Apply patches
  /// </summary>
  private void Awake()
  {
    Logger = base.Logger;
    Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} loaded");

    // TODO: Is there a simpler way to PatchAll()?
    //  Unless we give Harmony the Type, it doesn't seem to apply the patch.
    ApplyMenuPatches();
  }

  public static void ApplyMenuPatches()
  {
    Logger.LogInfo("Unapplying previous patches");
    Harmony.UnpatchID(MyPluginInfo.PLUGIN_GUID);

    Logger.LogInfo("Applying menu patches");
    foreach (Type patch in MenuPatches)
    {
      Logger.LogDebug($"Applying {patch.Name}");
      Harmony.CreateAndPatchAll(patch, MyPluginInfo.PLUGIN_GUID);
    }
  }

  /// <summary>
  /// Apply all our patches.
  /// Should only be done when we have successfully signed in
  /// </summary>
  public static void ApplyGameplayPatches()
  {
    Logger.LogInfo("Unapplying previous patches");
    Harmony.UnpatchID(MyPluginInfo.PLUGIN_GUID);

    Logger.LogInfo("Applying gameplay patches");
    foreach (Type patch in GameplayPatches)
    {
      Logger.LogDebug($"Applying {patch.Name}");
      Harmony.CreateAndPatchAll(patch, MyPluginInfo.PLUGIN_GUID);
    }
  }

  /// <summary>
  /// Unpatch all our patches
  /// </summary>
  public void UnpatchAll()
  {
    Logger.LogInfo("Unapplying patches");
    Harmony.UnpatchID(MyPluginInfo.PLUGIN_GUID);
  }

#if DEBUG
  private void Start()
  {
    StartCoroutine(nameof(CreateDebugMenu), 5.0f);
  }

  private IEnumerator CreateDebugMenu()
  {
    Logger.LogInfo("Creating debug menu");
    GameObject debugMenu = new("RhythmDoctor.Archipelago Debug");
    DontDestroyOnLoad(debugMenu);
    debugMenu.AddComponent<DebugMenu>();

    yield return null;
  }
#endif
}
