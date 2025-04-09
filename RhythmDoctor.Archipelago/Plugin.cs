namespace RhythmDoctor.Archipelago;

/// <summary>
/// Apply all patches and create the Logger
/// </summary>
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Rhythm Doctor.exe")]
public class Plugin : BaseUnityPlugin
{
  // ReSharper disable NullableWarningSuppressionIsUsed
  internal static Client.Client Client = null!;
  internal static new ManualLogSource Logger = null!;

  // ReSharper restore NullableWarningSuppressionIsUsed

  internal const string AlwaysActivePatchesID = $"{MyPluginInfo.PLUGIN_GUID}";
  internal const string ArchipelagoMenuPatchID = $"{MyPluginInfo.PLUGIN_GUID}.cls";
  internal const string PostLoginPatchesID = $"{MyPluginInfo.PLUGIN_GUID}.post";

  // Harmony's PatchCategories are not available on HarmonyX yet.

  /// <summary>
  /// Patches that are always applied regardless of Archipelago status.
  /// </summary>
  private static readonly Type[] AlwaysActivePatches =
  [
    typeof(ArchipelagoMenuOptionPatch),
    typeof(VersionTextPatch),
#if DEBUG
    typeof(LogClearLevelPatch),
#endif
  ];

  /// <summary>
  /// Patches that are applied after logging into Archipelago, and unapplied after logging out.
  /// </summary>
  private static readonly Type[] PostLoginPatches =
  [
    typeof(ArchipelagoMenuPatch),
    typeof(ClearLocationPatch),
    typeof(JanitorPatch),
    typeof(NicoleBlockagePatch),
    typeof(SkipTutorialPatch),
    //typeof(UnlockItemPatch),
  ];

  internal static readonly Type CustomLoginScreenPatch = typeof(ArchipelagoLoginPatch);

  /// <summary>
  /// Apply patches
  /// </summary>
  private void Awake()
  {
    Logger = base.Logger;
    Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} loaded");

    // TODO: Is there a simpler way to PatchAll()?
    //  Unless we give Harmony the Type, it doesn't seem to apply the patch.
    ApplyPatches(AlwaysActivePatches, AlwaysActivePatchesID);
  }

  private static void ApplyPatches(Type[] patches, string id)
  {
    Harmony harmony = new(id);

    Logger.LogInfo($"Applying {id} patches");
    foreach (Type patch in patches)
    {
      Logger.LogDebug($"Applying {patch.Name}");
      harmony.PatchAll(patch);
    }
  }

  internal static void UnapplyGameplayPatches()
  {
    Logger.LogInfo("Unapplying gameplay patches");
    Harmony.UnpatchID(PostLoginPatchesID);
  }

  internal static void ApplyArchipelagoMenuPatch()
  {
    Logger.LogInfo("Applying Archipelago menu patch");
    Harmony harmony = new(ArchipelagoMenuPatchID);
    harmony.PatchAll(CustomLoginScreenPatch);
  }

  internal static void UnapplyArchipelagoMenuPatch()
  {
    Logger.LogInfo("Unapplying Archipelago menu patch");
    Harmony.UnpatchID(ArchipelagoMenuPatchID);
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
