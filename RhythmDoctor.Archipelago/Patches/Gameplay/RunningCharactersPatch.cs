namespace RhythmDoctor.Archipelago.Patches.Gameplay;

[HarmonyPatch(typeof(scnLevelSelect))]
internal static class RunningCharactersPatch
{
  [HarmonyPatch(nameof(scnLevelSelect.UpdateCharacters))]
  [HarmonyPostfix]
  private static void FixRunningRinPatch(scnLevelSelect __instance)
  {
    if (Persistence.GetLevelRank(Level.BlackestLuxuryCar) == Rank.NotAvailable)
    {
      RDShaderProperties? shader = null;

      switch (__instance.currentWardIndex)
      {
        case scnLevelSelectExtensions.MUSE_DASH_AREA:
        {
          Plugin.Logger.LogDebug("Applying locked appearance to Rin");
          // csharpier-ignore
          shader = new RDShaderProperties(
            Color.black,
            RDConstants.data.levelSelect_lockedLevelTextOutline,
            1,
            true
          );
          break;
        }
        case scnLevelSelectExtensions.BASEMENT_AREA:
        {
          Plugin.Logger.LogDebug("Applying unlocked appearance to Rin");
          // csharpier-ignore
          shader = new RDShaderProperties(
            Color.clear,
            Color.black,
            1,
            true
          );
          break;
        }
      }

      if (shader == null)
      {
        return;
      }

      GameObject rinObject = GameObject.Find("/Scene/Corridor/GoToMuseDashRoom/Rin");
      scrChar rinChar = rinObject.GetComponent<scrChar>();
      rinChar.shaderData = shader;
      shader.SetFrameChanged();
    }
  }
}
