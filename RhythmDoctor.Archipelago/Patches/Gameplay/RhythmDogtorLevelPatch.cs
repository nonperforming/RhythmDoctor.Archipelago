namespace RhythmDoctor.Archipelago.Patches.Gameplay;

[HarmonyPatch]
internal static class RhythmDogtorLevelPatch
{
  // FIXME: Minor visual bug, Mr Stevendog doesn't show up properly on the pager.

  [HarmonyPatch(typeof(scnLevelSelect), nameof(scnLevelSelect.LoadLevelData))]
  [HarmonyPostfix]
  private static void AddRhythmDogtorPatch(scnLevelSelect __instance)
  {
    Plugin.Logger.LogDebug("Creating Rhythm Dogtor entity");

    // Visual
    SelectableCharacter character = (SelectableCharacter)__instance.FindSelectableEntity("3-X");
    character.bpmHard = character.bpmNormal;
    character.hardEnabled = true;
    character.charactersHard = [Character.Adog];
    // see GoToRhythmDogtorPatch, setting level to Lesmis causes rank to bug.
    character.levels[Difficulty.Hard] = Bindings.RHYTHM_DOGTOR_LEVEL;
    character.portraitsHard = [Character.MrStevendog, Character.Adog]; // FIXME: broken

    // Heart Monitor
    // Requires entry in GC.levelMetadata otherwise throws KeyNotFoundException
    if (!GC.levelMetadata.ContainsKey(Level.MyLevel))
    {
      Plugin.Logger.LogDebug("Creating Rhythm Dogtor metadata");

      LevelMetadata lesmisMetadata = GC.levelMetadata[Level.Lesmis];
      GuestData guestData1 = new() // Hameer Zawawi
      {
        link = lesmisMetadata.guest1.link,
        linkType = lesmisMetadata.guest1.linkType,
        name = lesmisMetadata.guest1.name,
        type = "Mix", // from Vocals and Mix
      };
      GuestData guestData2 = new() // Morphious86 - this replaces Kaisha who did the vocals for 3-X
      {
        link = "https://x.com/Morphious86",
        linkType = "Twitter",
        name = "Morphious86",
        type = "VocalsAndArt", // I mean... I guess? I think?
      };
      LevelMetadata rhythmDogtorMetadata = new()
      {
        id = "3-DOG",
        type = lesmisMetadata.type,
        portraits = [Character.MrStevendog, Character.Adog], // original: MrStevenson, Paige
        characters = [Character.Adog], // original: Paige
        guest1 = guestData1,
        guest2 = guestData2,
        guest3 = null,
        guest4 = null,
      };

      GC.levelMetadata.Add(Level.MyLevel, rhythmDogtorMetadata);
    }
  }

  [HarmonyPatch(typeof(scnLevelSelect), nameof(scnLevelSelect.GoToLevelSequence))]
  [HarmonyPrefix]
  private static void GoToRhythmDogtorPatch(ref string levelToGo, scnLevelSelect __instance)
  {
    if (__instance.currentDifficulty != Difficulty.Hard)
      return;
    if (RDUtils.ParseEnum<Level>(levelToGo) != Bindings.RHYTHM_DOGTOR_LEVEL)
      return;

    Plugin.Logger.LogInfo("Going to Rhythm Dogtor...");
    // Due to the loadDogMode check, this does not get overwritten to Lesmis.
    Persistence.SetLastPlayedLevel(Bindings.RHYTHM_DOGTOR_LEVEL);
    levelToGo = nameof(Level.Lesmis);
    scnGame.loadDogMode = true;
  }

  [HarmonyPatch(
    typeof(Persistence),
    nameof(Persistence.SetLevelRank),
    [typeof(string), typeof(Rank), typeof(bool), typeof(bool)]
  )]
  [HarmonyPrefix]
  private static void SaveRhythmDogtorRankPatch(ref string level)
  {
    if (scnGame.instance is null)
      return;
    if (level != nameof(Level.Lesmis))
      return;
    if (!scnGame.loadDogMode)
      return;

    Plugin.Logger.LogDebug("Saving Rhythm Dogtor rank...");
    level = Bindings.RHYTHM_DOGTOR_LEVEL.ToString();
  }

  /// <summary>
  /// Show Adog even if One Shift More has been cleared.
  /// </summary>
  [HarmonyPatch(typeof(scnLevelSelect), nameof(scnLevelSelect.SetDifficulty))]
  [HarmonyPrefix]
  private static void ShowAdogPatch(Difficulty newDifficulty, scnLevelSelect __instance)
  {
    // TODO: Art of Adog sleeping on the pet bed, maybe?
    // if (Persistence.GetLevelRank(Level.MyLevel).passed)
    // {
    // }
    bool passedLesmis = Persistence.GetLevelRank(Level.Lesmis).passed;
    switch (newDifficulty)
    {
      // FIXME: Can't find the logic to show Paige... logic may be incorrect
      case Difficulty.Normal:
        if (passedLesmis)
        {
          // Does Paige leaving in 6-X affect this?
          __instance.FindSelectableEntity("3-X").gameObject.SetActive(false);
          __instance.sleepingPaige.visible = true;
        }
        break;
      case Difficulty.Hard:
        // TODO: Somehow crop DogWard_Right and display it here
        //       It's seems to be using some old 'main ward' image (PT ward entrance looks different),
        //       we might need to edit the image.
        __instance.FindSelectableEntity("3-X").gameObject.SetActive(true);
        // xnopyt
        __instance.sleepingPaige.visible = false; // AA--
        break;
    }
  }

  // TODO: Implement Pulse localization already
  [HarmonyPatch(typeof(RDString), nameof(RDString.Get))]
  [HarmonyPrefix]
  private static void FixLocalizationHackPatch(string key, ref string __result, ref bool __runOriginal)
  {
    __runOriginal = false;
    switch (key)
    {
      case "levelSelect.MyLevel":
        __result = "Rhythm Dogtor";
        break;
      case "levelSelect.MyLevel.notPassed":
        __result = "Bark bark bark bark bark, bark bark bark. Bark bark bark bark bark bark bark bark.";
        break;
      case "levelSelect.MyLevel.passed":
        __result =
          "[Bark bark bark bark ba'rk bark bark bark-bark bark bark bark bark bark bark bark bark bark bark bark bark bark, bark bark bark bark bark bark… bark bark bark bark bark bark]";
        break;
      case "levelSelect.MyLevel.patient.notPassed":
        __result = "Whine :(";
        break;
      case "levelSelect.MyLevel.patient.passed":
        __result = "Bark. Bark bark, bark bark bark bark bark bark bark bark bark bark bark.";
        break;

      case "enum.GuestType.VocalsAndArt":
        __result = "Vocals and Art by ";
        break;

      default:
        __runOriginal = true;
        break;
    }
  }
}
