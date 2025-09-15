#if DEBUG
namespace RhythmDoctor.Archipelago.Debug.Patches;

[HarmonyPatch(typeof(RDStartup))]
internal class CreateDebugMenuPatch
{
  internal const string DEBUG_MENU_OBJECT_NAME = "RhythmDoctor.Archipelago Debug";

  // We create the Debug Menu object here instead of creating it during plugin startup (before commit 5c838d37cc)
  // as it seemingly breaks as of (Beta) build 18990163 (see https://steamdb.info/patchnotes/18990163/)
  // (Start method does not run?)
  [HarmonyPatch(nameof(RDStartup.Setup))]
  [HarmonyPostfix]
  private static void CreateDebugMenuAfterSetupPatch()
  {
    if (GameObject.Find($"/{DEBUG_MENU_OBJECT_NAME}") != null)
    {
      // Can get noisy, even seemingly innocent actions such as selecting entities in the Level Select calls
      // RDStartup.Setup multiple times.
      // Plugin.Logger.LogDebug("Debug menu object found, ignoring request to create new menu");
      return;
    }

    Plugin.Logger.LogInfo("Creating debug menu");
    GameObject debugMenu = new(DEBUG_MENU_OBJECT_NAME);
    UnityEngine.Object.DontDestroyOnLoad(debugMenu);
    debugMenu.AddComponent<DebugMenu>();
    Plugin.DebugMenu = debugMenu.GetComponent<DebugMenu>();
  }
}
#endif
