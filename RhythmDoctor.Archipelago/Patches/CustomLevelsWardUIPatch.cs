namespace RhythmDoctor.Archipelago.Patches;

/// <summary>
/// Handle selecting custom tabs on the Custom Levels Ward
/// </summary>
[HarmonyPatch(typeof(scnCLS))]
internal static class CustomLevelsWardUIPatch
{
  internal static Dictionary<int, Action> LevelWardOptions = [];

  [HarmonyPatch(nameof(scnCLS.SelectWardOption))]
  [HarmonyPostfix]
  static void Postfix(scnCLS __instance)
  {
    foreach (KeyValuePair<int, Action> pair in LevelWardOptions)
    {
      if ((int)scnCLS.instance.CurrentWardOption.name == pair.Key)
      {
        pair.Value();
        break;
      }
    }
  }
}
