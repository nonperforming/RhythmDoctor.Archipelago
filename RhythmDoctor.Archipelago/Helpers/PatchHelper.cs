namespace RhythmDoctor.Archipelago.Helpers;

internal static class PatchHelper
{
  internal const string PATCH_ID_ALWAYS_ACTIVE = MyPluginInfo.PLUGIN_GUID;
#if DEBUG
  internal const string PATCH_ID_DEBUG = $"{MyPluginInfo.PLUGIN_GUID}.debug";
#endif
  internal const string PATCH_ID_ARCHIPELAGO_MENU = $"{MyPluginInfo.PLUGIN_GUID}.cls";
  internal const string PATCH_ID_POST_LOGIN = $"{MyPluginInfo.PLUGIN_GUID}.post";
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

  internal static void ApplyPatches(string id, params IEnumerable<Type> patches)
  {
    Plugin.Logger.LogInfo($"[{nameof(PatchHelper)}] Applying patches as {id}");
    Harmony harmony = new(id);

    foreach (Type patch in patches)
    {
      Plugin.Logger.LogInfo($"[{nameof(PatchHelper)}] Applying {patch.Name}");
      harmony.PatchAll(patch);
    }
  }

  internal static void ApplyAlwaysActivePatches()
  {
    Plugin.Logger.LogInfo($"[{nameof(PatchHelper)}] Applying always active patches");
    ApplyPatches(PATCH_ID_ALWAYS_ACTIVE, AlwaysActivePatches);
  }

  // Applying any post-login patches are done in their respective client.

  internal static void UnapplyPostLoginPatches()
  {
    Plugin.Logger.LogInfo($"[{nameof(PatchHelper)}] Unapplying post-login patches");
    Harmony.UnpatchID(PATCH_ID_POST_LOGIN);
    Harmony.UnpatchID(PATCH_ID_SLEEVE_PAINT);
  }

#if DEBUG
  internal static void UnapplyDebugPatches()
  {
    Plugin.Logger.LogInfo($"[{nameof(PatchHelper)}] Unapplying debug patches");
    Harmony.UnpatchID(PATCH_ID_DEBUG);
  }
#endif

  internal static void ApplyArchipelagoMenuPatch()
  {
    Plugin.Logger.LogInfo($"[{nameof(PatchHelper)}] Applying Archipelago menu patch");
    Harmony.CreateAndPatchAll(typeof(ArchipelagoLoginPatch), PATCH_ID_ARCHIPELAGO_MENU);
  }

  internal static void UnapplyArchipelagoMenuPatch()
  {
    Plugin.Logger.LogInfo("Unapplying Archipelago menu patch");
    Harmony.UnpatchID(PATCH_ID_ARCHIPELAGO_MENU);
  }

  /// <remarks>
  /// This does not unapply any modifier patches; these should be handled by its manager.
  /// </remarks>
  internal static void UnapplyAllPatches()
  {
    Harmony.UnpatchID(PATCH_ID_ALWAYS_ACTIVE);
    Harmony.UnpatchID(PATCH_ID_ARCHIPELAGO_MENU);
    Harmony.UnpatchID(PATCH_ID_POST_LOGIN);
  }
}
