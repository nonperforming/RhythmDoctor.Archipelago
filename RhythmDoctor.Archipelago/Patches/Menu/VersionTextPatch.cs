namespace RhythmDoctor.Archipelago.Patches;

/// <summary>
/// Append our plugin's version to Rhythm Doctor's version on the home screen.
/// </summary>
[HarmonyPatch(typeof(RDVersionText))]
static class VersionTextPatch
{
  [HarmonyPatch(nameof(RDVersionText.SetPage))]
  [HarmonyPostfix]
  static void Postfix(RDVersionText __instance)
  {
    string text = __instance.text.text += " / Archipelago v" + MyPluginInfo.PLUGIN_VERSION;

#if DEBUG
    text += "D";
#endif

    __instance.text.text = text;
  }
}
