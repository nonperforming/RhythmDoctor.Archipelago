namespace RhythmDoctor.Archipelago.Patches.Gameplay;

[HarmonyPatch(typeof(scnLevelSelect))]
internal static class RunningCharactersPatch
{
  /// <summary>
  /// The ward index for the Basement area.
  /// </summary>
  /// <seealso cref="scnLevelSelect.currentWardIndex"/>
  private const int BASEMENT_AREA = 3;

  /// <summary>
  /// The ward index for the Muse Dash area.
  /// </summary>
  /// <seealso cref="scnLevelSelect.currentWardIndex"/>
  private const int MUSE_DASH_AREA = 7;

  [HarmonyPatch(nameof(scnLevelSelect.UpdateCharacters))]
  [HarmonyPostfix]
  private static void FixRunningRinPatch(scnLevelSelect __instance)
  {
    if (Persistence.GetLevelRank(Level.BlackestLuxuryCar) == Rank.NotAvailable)
    {
      RDShaderProperties? shader = null;

      switch (__instance.currentWardIndex)
      {
        case MUSE_DASH_AREA:
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
        case BASEMENT_AREA:
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
