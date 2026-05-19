namespace RhythmDoctor.Archipelago.Patches;

/// <summary>
/// Randomize the player's sleeve until they receive a Sleeve Paint item and unapply this patch.
/// </summary>
/// <remarks>
/// This patch should be applied under the ID <see cref="Plugin.PATCH_ID_SLEEVE_PAINT"/>.
/// </remarks>
/// <seealso cref="ClientOld.ProcessItem"/>
[HarmonyPatch(typeof(ArmSkin))]
internal static class LockSleevePaintPatch
{
  [HarmonyPatch(nameof(ArmSkin.Load))]
  [HarmonyPrefix]
  private static void LoadDefaultSleevePatch(ref ArmSkin __instance, ref bool __runOriginal)
  {
    __runOriginal = false;

    int slot = Plugin.Random.Next(0, 2);

    // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
    switch (__instance.player)
    {
      case RDPlayer.P1:
        __instance.skinColor = RDConstants.data.skinColors[RDConstants.data.defaultP1SkinColor[slot]];
        __instance.sleeveColor = RDConstants.data.sleeveColors[RDConstants.data.defaultP1SleeveColor[slot]];
        __instance.nailsColor = RDConstants.data.nailColors[RDConstants.data.defaultP1NailColor[slot]];
        __instance.palmLightness = RDConstants.data.defaultP1PalmLightness[slot];
        __instance.drawing = RDConstants.data.emptyTexture;
        break;
      case RDPlayer.P2:
        __instance.skinColor = RDConstants.data.skinColors[RDConstants.data.defaultP2SkinColor[slot]];
        __instance.sleeveColor = RDConstants.data.sleeveColors[RDConstants.data.defaultP2SleeveColor[slot]];
        __instance.nailsColor = RDConstants.data.nailColors[RDConstants.data.defaultP2NailColor[slot]];
        __instance.palmLightness = RDConstants.data.defaultP2PalmLightness[slot];
        __instance.drawing = RDConstants.data.emptyTexture;
        break;
    }
  }
}
