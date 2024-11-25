namespace RhythmDoctor.Archipelago.Patches;

/// <summary>
/// Force all Janitors to be visible.
/// </summary>
internal static class JanitorPatch
{
  //  [HarmonyPostfix]
  static void Postfix()
  {
    //    __instance.text.text = __instance.text.text + " / Archipelago v" + MyPluginInfo.PLUGIN_VERSION;
  }
}
