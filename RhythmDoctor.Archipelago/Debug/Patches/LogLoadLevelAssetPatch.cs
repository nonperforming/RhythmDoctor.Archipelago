// FIXME
/*
#if DEBUG
namespace RhythmDoctor.Archipelago.Debug.Patches;

internal static class LogLoadLevelAsset
{
  [HarmonyPatch(typeof(LevelBase), nameof(LevelBase.LoadLevelAsset))]
  [HarmonyPrefix]
  internal static void LogStringLoadLevelAssetPatch(ref LevelBase __instance, string name)
  {
    Plugin.Logger.LogInfo(
      @$"--- LevelBase.LoadLevelAsset(string name = {name})
Type: {__instance.levelType}
Invisible Characters: {__instance.invisibleChars}
Invisible Hearts: {__instance.invisibleHeart}
Level To Skip To: {__instance.levelToSkipTo}
Skippable: {__instance.skippable}
Custom Game Over: {__instance.customGameover}
Dog Mode: {__instance.dogMode}
---"
    );
  }*/

/*
//   This seems to be used for a lot of stuff!
//   typeof(T) = RDSpaceBackground
//             = RDBoyWard
//             =
[HarmonyPatch(typeof(RDSpaceBackground), nameof(LevelBase.LoadLevelAsset))]
[HarmonyPatch(typeof(RDBoyWard), nameof(LevelBase.LoadLevelAsset))]
[HarmonyPrefix]
internal static void LogGenericLoadLevelAssetPatch(ref LevelBase __instance)
{
  Type? type = __instance.GetType().DeclaringType;
  string name = (type != null) ? type.Name : "Unknown";

  Plugin.Logger.LogInfo(
    @$"--- LevelBase.LoadLevelAsset<{type}>()
Name: {name}
Type: {__instance.levelType}
Invisible Characters: {__instance.invisibleChars}
Invisible Hearts: {__instance.invisibleHeart}
Level To Skip To: {__instance.levelToSkipTo}
Skippable: {__instance.skippable}
Custom Game Over: {__instance.customGameover}
Dog Mode: {__instance.dogMode}
---"
  );
}
*/
/*}
#endif
*/
