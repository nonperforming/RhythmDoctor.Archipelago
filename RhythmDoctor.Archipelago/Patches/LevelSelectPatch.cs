namespace RhythmDoctor.Archipelago.Patches;

/// <summary>
/// Make all levels and wards available
/// </summary>
[HarmonyPatch(typeof(scnLevelSelect))]
internal static class LevelSelectPatch
{
  private static bool patched = false;

  [HarmonyPatch("LateUpdate")]
  [HarmonyPostfix]
  static void Prefix(scnLevelSelect __instance)
  {
    if (patched) return;

    // Make all levels visible
    foreach (Level level in Enum.GetValues(typeof(Level)))
    {
      if (level == Level.OrientalTechno) return; // Don't lock 1-1 - Oriental Techno: it is always available
      Persistence.SetLevelRank(level, Rank.NotAvailable);
    }

    // Unlock all wards/rooms
    __instance.UnlockEntrance(FindSelectableEntity(__instance, "GoToArtRoom"));
    __instance.UnlockEntrance(FindSelectableEntity(__instance, "GoToAthleteWard"));
    __instance.UnlockEntrance(FindSelectableEntity(__instance, "GoToBasement"));
    __instance.UnlockEntrance(FindSelectableEntity(__instance, "GoToMuseDashRoom"));
    __instance.UnlockEntrance(FindSelectableEntity(__instance, "GoToRooftop"));
    __instance.UnlockEntrance(FindSelectableEntity(__instance, "GoToSVTWard"));
    __instance.UnlockEntrance(FindSelectableEntity(__instance, "GoToTrain"));

    //Persistence.SetIsGameDone(state: true);

    Persistence.SaveCurrentSlot();
    patched = true;
  }

  [HarmonyReversePatch]
  [HarmonyPatch("FindSelectableEntity")]
  public static SelectableEntity FindSelectableEntity(object instance, string name)
    => throw new NotImplementedException("Stub method called");
}
