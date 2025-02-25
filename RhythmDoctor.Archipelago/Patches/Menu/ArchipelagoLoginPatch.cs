using Logger = UnityEngine.Logger;

namespace RhythmDoctor.Archipelago.Patches.Menu;

internal static class ArchipelagoLoginPatch
{
  // FIXME: This isn't matching for some reason!
  // [HarmonyPatch(typeof(scnCLS), nameof(scnCLS.Awake))]
  // [HarmonyTranspiler]
  // private static IEnumerable<CodeInstruction> ShowAllWardOptions(IEnumerable<CodeInstruction> instructions)
  // {
  //   // Steam ward option (if Steam isn't initialized) and Import ward option (if on the Steam Deck)
  //   return new CodeMatcher()
  //     // if (!SteamIntegration.initialized)
  //     .MatchForward(false, new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(SteamIntegration), nameof(SteamIntegration.initialized))))
  //     .SetOpcodeAndAdvance(OpCodes.Nop) // ldsfld bool SteamIntegration::initialized
  //     .SetOpcodeAndAdvance(OpCodes.Nop) // brtrue.s IL_01d3
  //     // if (Persistence.GetFeatureSet() == FeatureSet.SteamDeck)
  //     .MatchForward(false, new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Persistence), nameof(Persistence.GetFeatureSet))))
  //     .SetOpcodeAndAdvance(OpCodes.Nop) // call valuetype FeatureSet Persistence::GetFeatureSet()
  //     .SetOpcodeAndAdvance(OpCodes.Nop) // ldc.i4.1
  //     .SetOpcodeAndAdvance(OpCodes.Nop) // bne.un.s IL_022e
  //     .InstructionEnumeration();
  // }

  [HarmonyPatch(typeof(scnCLS), nameof(scnCLS.Awake))]
  [HarmonyPostfix]
  private static void RenameWardOptions(ref scnCLS __instance)
  {
    // We patch Start instead of Awake otherwise we get strange bugs with Finding certain objects
    Plugin.Logger.LogInfo("Renaming ward options");

    // Get WardOptions
    scnCLS.WardOption libraryOption = __instance.wardOptions.Find(wardOption =>
      wardOption.name == scnCLS.WardOptionName.Library
    );
    scnCLS.WardOption workshopOption = __instance.wardOptions.Find(wardOption =>
      wardOption.name == scnCLS.WardOptionName.OpenSteamWorkshop
    );
    scnCLS.WardOption importOption = __instance.wardOptions.Find(wardOption =>
      wardOption.name == scnCLS.WardOptionName.ImportLevels
    );

    // Delete Library and Steam Workshop options
    libraryOption.rect.transform.parent.gameObject.SetActive(false);
    workshopOption.rect.transform.parent.gameObject.SetActive(false);
    __instance.wardOptions.Remove(libraryOption);
    __instance.wardOptions.Remove(workshopOption);

    // Import to Archipelago option
    //importOption.rect.
  }

  [HarmonyPatch(typeof(scnCLS), nameof(scnCLS.SelectWardOption))]
  [HarmonyPostfix]
  private static void CustomSelectOption(ref bool __runOriginal, scnCLS __instance)
  {
    __runOriginal = false;
    switch (__instance.CurrentWardOption.name)
    {
      case scnCLS.WardOptionName.Library:
      case scnCLS.WardOptionName.OpenSteamWorkshop:
        // It should not be possible to select these, but just in case.
        return;
      default:
        // Exit or import options
        __runOriginal = true;
        break;
    }
  }
}
