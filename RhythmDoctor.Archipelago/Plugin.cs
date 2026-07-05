namespace RhythmDoctor.Archipelago;

/// <summary>
/// Archipelago client mod for Rhythm Doctor
/// </summary>
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(PulseLib.MyPluginInfo.PLUGIN_GUID, PulseLib.MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Rhythm Doctor.exe")]
public class Plugin : BaseUnityPlugin
{
  // ReSharper disable NullableWarningSuppressionIsUsed
  internal static StoryClient StoryClient = null!;
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
    typeof(scnGameExtensions),
#if DEBUG
    typeof(CreateDebugMenuPatch),
    typeof(LogClearLevelPatch),
    typeof(LogEntityInteractionsPatch),
    // typeof(LogLoadLevelAssetPatch),
#endif
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

    Logger.LogInfo("Setting up paths");
    Paths.PopulatePaths();

    Logger.LogInfo($"Registering custom localization ({Paths.Localization})");
    CustomLocalizationHelper.SearchAndRegisterDirectory(Paths.Localization);

    Logger.LogInfo("Applying always active patches");
    ApplyPatches(PATCH_ID_ALWAYS_ACTIVE, AlwaysActivePatches);

    Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} loaded");
  }

  /// <summary>
  /// Show update banner if necessary.
  /// </summary>
  private void Start()
  {
    // TODO: Find out some way to automate this.
    //       Source generation or something?????
    //       It would require we pull from game files though...
    //       ...it wouldn't work on CI.
    //       Probably write a script or something that just fetches the required resources and outputs a C# file.
    Version builtForVersion = new("1.1.1");
    const int RELEASE_NUMBER = 42;
    const string RELEASE_HASH = "e43207b";
    const string RELEASE_DATE = "2026/06/15 12:42 AM";

    // https://patorjk.com/software/taag/#p=display&f=Future+Smooth&t=Please+update+your+game!!!
    // TODO: parse date for same version and release number but differing hash
    Version thisVersion = new(Application.version);
    if (Releases.releaseNumber < RELEASE_NUMBER || thisVersion < builtForVersion)
    {
      // csharpier-ignore-start
      Logger.LogWarning( "================================================================================");
      Logger.LogWarning( "|   ╭─╮╷  ╭─╴╭─╮╭─╮╭─╴   ╷ ╷╭─╮╶┬╮╭─╮╶┬╴╭─╴   ╷ ╷╭─╮╷ ╷╭─╮   ╭─╴╭─╮╭┬╮╭─╴╷╷╷   |");
      Logger.LogWarning( "|   ├─╯│  ├╴ ├─┤╰─╮├╴    │ │├─╯ ││├─┤ │ ├╴    ╰┬╯│ ││ │├┬╯   │╶╮├─┤│││├╴ ╵╵╵   |");
      Logger.LogWarning( "|   ╵  ╰─╴╰─╴╵ ╵╰─╯╰─╴   ╰─╯╵  ╶┴╯╵ ╵ ╵ ╰─╴    ╵ ╰─╯╰─╯╵╰╴   ╰─╯╵ ╵╵ ╵╰─╴╵╵╵   |");
      Logger.LogWarning( "================================================================================");
      Logger.LogWarning($"This version of the mod (v{MyPluginInfo.PLUGIN_VERSION}) was built for: v{builtForVersion} (release {RELEASE_NUMBER}, commit {RELEASE_HASH}, date {RELEASE_DATE})");
      Logger.LogWarning($"Your version is: v{Application.version} (release {Releases.releaseNumber}, commit {Releases.buildCommit}, date {Releases.buildDate})");
      Logger.LogWarning( "================================================================================");
      // csharpier-ignore-end
    }
    else if (Releases.releaseNumber > RELEASE_NUMBER || thisVersion > builtForVersion)
    {
      // csharpier-ignore-start
      Logger.LogWarning( "=================================================================================");
      Logger.LogWarning( "|         ╭─╮╷  ╭─╴╭─╮╭─╮╭─╴   ╭─╮╭─╮╭─╴╭╮╷   ╭─╮╭╮╷   ╷╭─╮╭─╮╷ ╷╭─╴╷╷╷         |");
      Logger.LogWarning( "|         ├─╯│  ├╴ ├─┤╰─╮├╴    │ │├─╯├╴ │╰┤   ├─┤│╰┤   │╰─╮╰─╮│ │├╴ ╵╵╵         |");
      Logger.LogWarning( "|         ╵  ╰─╴╰─╴╵ ╵╰─╯╰─╴   ╰─╯╵  ╰─╴╵ ╵   ╵ ╵╵ ╵   ╵╰─╯╰─╯╰─╯╰─╴╵╵╵         |");
      Logger.LogWarning( "=================================================================================");
      Logger.LogWarning($"This version of the mod (v{MyPluginInfo.PLUGIN_VERSION}) was built for: v{builtForVersion} (release {RELEASE_NUMBER}, commit {RELEASE_HASH}, date {RELEASE_DATE})");
      Logger.LogWarning($"Your version is: v{Application.version} (release {Releases.releaseNumber}, commit {Releases.buildCommit}, date {Releases.buildDate})");
      Logger.LogWarning(null);
      Logger.LogWarning( "First, please check that there is an updated version here: https://github.com/nonperforming/RhythmDoctor.Archipelago/releases. Remember to update Pulse (https://github.com/nonperforming/Pulse/releases) as well.");
      Logger.LogWarning($"If there is no update yet, please open an issue at https://github.com/nonperforming/RhythmDoctor.Archipelago/issues/new?labels=\"plugin outdated\"&title=v{Application.version}+support&body=Release:+{Releases.releaseNumber}+/+Commit:+{Releases.buildCommit}+/+Date:+{Releases.buildDate}&assignees=nonperforming");
      Logger.LogWarning( "=================================================================================");
      // csharpier-ignore-end
    }
  }

  private void Update()
  {
    while (ToExecuteOnMainThread.TryDequeue(out Action action))
    {
      action.Invoke();
    }
  }

  // TODO: see storyclient L144 ("pull this out")
  internal static void ApplyPatches(string id, params Type[] patches)
  {
    Logger.LogInfo($"Applying patches as {id}");
    Harmony harmony = new(id);

    foreach (Type patch in patches)
    {
      Logger.LogInfo($"Applying {patch.Name}");
      harmony.PatchAll(patch);
    }
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
