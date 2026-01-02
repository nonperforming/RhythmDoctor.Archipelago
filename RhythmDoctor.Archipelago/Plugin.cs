namespace RhythmDoctor.Archipelago;

/// <summary>
/// Archipelago client mod for Rhythm Doctor
/// </summary>
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(PulseLib.MyPluginInfo.PLUGIN_GUID)]
[BepInProcess("Rhythm Doctor.exe")]
public class Plugin : BaseUnityPlugin
{
  // ReSharper disable NullableWarningSuppressionIsUsed
  internal static Client.Client Client = null!;
  internal static new ManualLogSource Logger = null!;
#if DEBUG
  internal static DebugMenu DebugMenu = null!;
#endif

  internal static readonly Random Random = new();
  internal static readonly ConcurrentQueue<Action> ToExecuteOnMainThread = new();

  internal static Plugin Instance = null!;

  // ReSharper restore NullableWarningSuppressionIsUsed

  internal const string PATCH_ID_ALWAYS_ACTIVE = MyPluginInfo.PLUGIN_GUID;
#if DEBUG
  internal const string PATCH_ID_DEBUG = $"{MyPluginInfo.PLUGIN_GUID}.debug";
#endif
  internal const string PATCH_ID_ARCHIPELAGO_MENU = $"{MyPluginInfo.PLUGIN_GUID}.cls";
  internal const string PATCH_ID_POST_LOGIN = $"{MyPluginInfo.PLUGIN_GUID}.post";
  internal const string PATCH_ID_TRAP = $"{MyPluginInfo.PLUGIN_GUID}.trap";
  internal const string PATCH_ID_SLEEVE_PAINT = $"{MyPluginInfo.PLUGIN_GUID}.sleevepaint";

  // Harmony's PatchCategories are not available on HarmonyX yet.

  /// <summary>
  /// Patches that are always applied regardless of Archipelago status.
  /// </summary>
  private static readonly Type[] AlwaysActivePatches =
  [
    typeof(ArchipelagoMainMenuOptionPatch),
    typeof(VersionTextPatch),
#if DEBUG
    typeof(CreateDebugMenuPatch),
    typeof(LogClearLevelPatch),
    typeof(LogEntityInteractionsPatch),
    // typeof(LogLoadLevelAssetPatch),
#endif
  ];

  /// <summary>
  /// Patches that are applied after logging into Archipelago, and unapplied after logging out.
  /// </summary>
  private static readonly Type[] PostLoginPatches =
  [
    typeof(Act5Patch),
    typeof(ClearLocationPatch),
    typeof(DeathLinkPatch),
    //typeof(JanitorPatch), // use pause menu for in/outbox
    typeof(LevelSelectVisualFixesPatch),
    typeof(RhythmWeightlifterPatch),
    typeof(RunningCharactersPatch),
    typeof(SkipCutscenePatch),
    typeof(SkipTutorialPatch),
    typeof(StateReplicationPatch),
    typeof(TrapManagerPatch),
    typeof(UnlockItemPatch),
    typeof(WelcomeBackPatch),
    typeof(SavingPatch),
    typeof(UnapplyPatchesPatch),
  ];

  private static readonly Type CustomLoginScreenPatch = typeof(ArchipelagoLoginPatch);

  /// <summary>
  /// Apply AlwaysActive patches and create the Logger
  /// </summary>
  private void Awake()
  {
    Instance = this;
    Logger = base.Logger;
    Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} loading");

    Configuration.Bind(Config);

    // TODO: Fix Pulse localization first
    // Logger.LogInfo("Registering custom localization");
    // LocalizationHelpers.RegisterJson(LangCode.English, DataHelper.GetLocalizationJson(LangCode.English));

    // TODO: Is there a simpler way to PatchAll()?
    //  Unless we give Harmony the Type, it doesn't seem to apply the patch.
    Logger.LogInfo("Applying always active patches");
    ApplyPatches(AlwaysActivePatches, PATCH_ID_ALWAYS_ACTIVE);

    Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} loaded");
  }

  private void Update()
  {
    while (ToExecuteOnMainThread.TryDequeue(out Action action))
    {
      action.Invoke();
    }
  }

  private static void ApplyPatches(Type[] patches, string id)
  {
    Logger.LogInfo($"Applying patches as {id}");
    Harmony harmony = new(id);

    foreach (Type patch in patches)
    {
      Logger.LogDebug($"Applying {patch.Name}");
      harmony.PatchAll(patch);
    }
  }

  internal static void ApplyGameplayPatches()
  {
    Logger.LogInfo("Applying gameplay patches");
    ApplyPatches(PostLoginPatches, PATCH_ID_POST_LOGIN);
    Harmony.CreateAndPatchAll(typeof(LockSleevePaintPatch), PATCH_ID_SLEEVE_PAINT);
  }

  internal static void UnapplyGameplayPatches()
  {
    Logger.LogInfo("Unapplying gameplay patches");
    Harmony.UnpatchID(PATCH_ID_POST_LOGIN);
    Harmony.UnpatchID(PATCH_ID_SLEEVE_PAINT);
  }

#if DEBUG
  internal static void UnapplyDebugPatches()
  {
    Logger.LogInfo("Unapplying debug patches");
    Harmony.UnpatchID(PATCH_ID_DEBUG);
  }
#endif

  internal static void ApplyArchipelagoMenuPatch()
  {
    Logger.LogInfo("Applying Archipelago menu patch");
    Harmony.CreateAndPatchAll(CustomLoginScreenPatch, PATCH_ID_ARCHIPELAGO_MENU);
  }

  internal static void UnapplyArchipelagoMenuPatch()
  {
    Logger.LogInfo("Unapplying Archipelago menu patch");
    Harmony.UnpatchID(PATCH_ID_ARCHIPELAGO_MENU);
  }

  #region Cleaning up
  private bool _quitting = false;

  private void OnApplicationQuit()
  {
    Logger.LogDebug("Quitting...");
    _quitting = true;
  }

  private void OnDestroy()
  {
    if (_quitting)
      return;

    Logger.LogWarning("Tearing down plugin. This is unsupported!");
    UnapplyPatchesPatch.TearDownClientPluginPatch();
    Harmony.UnpatchID(PATCH_ID_ALWAYS_ACTIVE);
    Harmony.UnpatchID(PATCH_ID_ARCHIPELAGO_MENU);
    Harmony.UnpatchID(PATCH_ID_POST_LOGIN);
    Harmony.UnpatchID(PATCH_ID_TRAP);
#if DEBUG
    DestroyImmediate(GameObject.Find($"/{CreateDebugMenuPatch.DEBUG_MENU_OBJECT_NAME}"));
#endif
  }
  #endregion
}
