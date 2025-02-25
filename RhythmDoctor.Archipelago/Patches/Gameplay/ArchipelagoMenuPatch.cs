namespace RhythmDoctor.Archipelago.Patches.Gameplay;

/// <summary>
/// Handle selecting custom tabs on the Custom Levels Ward
/// </summary>
[HarmonyPatch(typeof(scnCLS))]
internal static class ArchipelagoMenuPatch
{
  // TODO: Implement
  internal static Dictionary<int, Action> CustomLevelWardOptions = [];

  [HarmonyPatch(nameof(scnCLS.SelectWardOption))]
  [HarmonyPostfix]
  static void Postfix(scnCLS __instance)
  {
    foreach (KeyValuePair<int, Action> pair in CustomLevelWardOptions)
    {
      if ((int)scnCLS.instance.CurrentWardOption.name == pair.Key)
      {
        pair.Value();
        break;
      }
    }
  }
}
