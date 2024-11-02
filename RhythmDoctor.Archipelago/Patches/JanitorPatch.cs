namespace RhythmDoctor.Archipelago.Patches;

//[HarmonyPatch(typeof(RDVersionText), "SetPage")]
/// <summary>
/// TODO
/// </summary>
internal static class JanitorPatch
{
  //  [HarmonyPostfix]
  static void Postfix()
  {
    //    __instance.text.text = __instance.text.text + " / Archipelago v" + MyPluginInfo.PLUGIN_VERSION;
  }
}
