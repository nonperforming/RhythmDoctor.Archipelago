#if DEBUG
namespace RhythmDoctor.Archipelago.Debug.Patches;

[HarmonyPatch(typeof(RDStartup))]
class CreateDebugMenuPatch
{
  internal const string DEBUG_MENU_OBJECT_NAME = "RhythmDoctor.Archipelago Debug";

  [HarmonyPatch(nameof(RDStartup.Setup))]
  [HarmonyPostfix]
  static void CreateDebugMenuAfterSetupPatch()
  {
    if (GameObject.Find($"/{DEBUG_MENU_OBJECT_NAME}") != null)
    {
      Plugin.Logger.LogDebug("Debug menu object found, ignoring request to create new menu");
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
