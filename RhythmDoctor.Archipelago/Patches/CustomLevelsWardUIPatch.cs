using System.Reflection;

namespace RhythmDoctor.Archipelago.Patches;

/// <summary>
/// Handle custom tabs on the Custom Levels Ward
/// </summary>
[HarmonyPatch(typeof(scnCLS))]
internal static class CustomLevelsWardUIPatch
{
  internal static Dictionary<int, Action> LevelWardOptions = [];

  [HarmonyPatch("SelectWardOption")]
  [HarmonyPostfix]
  static void Postfix(scnCLS __instance)
  {
    // Access private WardOption CurrentWardOption in scnCLS
    PropertyInfo propertyInfo = typeof(scnCLS).GetProperty("CurrentWardOption", BindingFlags.NonPublic);
    MethodInfo getMethod = propertyInfo.GetGetMethod(true);
    scnCLS.WardOption currentWardOption = (scnCLS.WardOption)getMethod.Invoke(__instance, null);

    foreach (KeyValuePair<int, Action> pair in LevelWardOptions)
    {
      if ((int)currentWardOption.name == pair.Key)
      {
        pair.Value();
        break;
      }
    }
  }

  [HarmonyReversePatch]
  [HarmonyPatch("CurrentWardOption", MethodType.Getter)]
  internal static scnCLS.WardOption CurrentWardOptionGetter()
  {
    throw new NotImplementedException("Stub method called");
  }
}
