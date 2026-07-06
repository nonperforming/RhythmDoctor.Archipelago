using System.Reflection;
using System.Text.RegularExpressions;

namespace RhythmDoctor.Archipelago.Patches.Gameplay;

[HarmonyPatch(typeof(scnLevelSelect))]
internal static class UnlockItemPatch
{
  [HarmonyPatch(nameof(scnLevelSelect.LoadLevelData))]
  [HarmonyPostfix]
  private static void UnlockBonusItemsPatch(scnLevelSelect __instance)
  {
    // TODO: Bit inefficient

    // Processing new items...
    Plugin.StoryClient.ProcessNewReceivedItems();
    
    // This is a PostLogin patch, session is guaranteed to exist (assuming going through normal flow)
    Plugin.Logger.LogInfo($"[{nameof(UnlockItemPatch)}] Unlocking bonus items");

    // Unlocking regions and Sleeve Paint
    Plugin.Logger.LogInfo($"[{nameof(UnlockItemPatch)}] Checking for regions to unlock");

    bool hasBasementKey = false;
    
    // TODO: remove once we've fully migrated to using storyclient.
    // ReSharper disable once NullableWarningSuppressionIsUsed
    foreach (ItemInfo item in Plugin.StoryClient.Session!.Items.AllItemsReceived)
    {
      Plugin.Logger.LogDebug($"[{nameof(UnlockItemPatch)}] Processing item {item.ItemName} ({item.ItemId})");
      if (Bindings.KeyItemIdToWard.TryGetValue(item.ItemId, out Region region))
      {
        if (region == Region.Basement)
        {
          hasBasementKey = true;
        }
        Plugin.Logger.LogInfo($"[{nameof(UnlockItemPatch)}] Unlocking entrance {region}");
        scnLevelSelect.instance.UnlockEntrance(region);
      }
      else if (Bindings.ItemIdToLevel.TryGetValue(item.ItemId, out Level level))
      {
        Plugin.Logger.LogInfo(
          $"[{nameof(UnlockItemPatch)}] Unlocking stage item {item.ItemName} ({item.ItemId}, {level})"
        );
        Persistence.SetLevelRank(level, Rank.NotFinished, false, false);
      }
    }

    if (!hasBasementKey)
    {
      Plugin.Logger.LogInfo($"[{nameof(UnlockItemPatch)}] Locking basement");
      __instance.LockEntrance(scnLevelSelect.GoToBasement);
    }

    // Moving 1-CNY.
    // If we do not do this, 1-CNY and 1-BOO will overlap each other.
    Plugin.Logger.LogInfo("Moving 1-CNY");
    __instance.FindSelectableEntity("1-CNY").gamePosition.x = -564;

    if (Plugin.StoryClient.Slot.endGoal == SlotData.EndGoal.HelpingHands)
    {
      // Moving X-1 - Art Exercise to the basement if end goal is X-0 - Helping Hands
      Plugin.Logger.LogInfo("Moving X-1 to the basement");
      SelectableEntity artExercise = __instance.FindSelectableEntity("X-1");
      // Moving from group 6 (Art Room) to 3 (Basement)
      artExercise.group = scnLevelSelectExtensions.BASEMENT_AREA;
      // Set selection order to just after the basement computer. This is separate from world position.
      int index =
        __instance.selectableEntities.FindIndex(entity => entity.gameObject.name == scnLevelSelect.BasementComputer)
        + 1;
      artExercise.gamePosition = new Vector2(2480, 53); // in front of Ian's bookshelf/archive thing
      __instance.selectableEntities.Remove(artExercise);
      __instance.selectableEntities.Insert(index, artExercise);

      // Unlock the Art Room (allow access to X-0 - Helping Hands) if we have all bosses completed.
      if (Bindings.ActBoss.Values.All(levels => levels.All(level => Persistence.GetLevelRank(level).passed)))
      {
        Plugin.Logger.LogInfo($"[{nameof(UnlockItemPatch)}] Unlocking Art Room and X-0 - Helping Hands");
        __instance.UnlockEntrance(__instance.FindSelectableEntity(scnLevelSelect.GoToArtRoom));
        Persistence.SetLevelRank(Level.HelpingHands, Rank.NotFinished, false, false);
      }
    }

    #region Unhiding levels
    // Show all Basement levels
    // Do not pull in X-0 and X-1 (and any future non-collab levels)
    foreach (
      SelectableEntity level in __instance.selectableEntities.Where(
        (entity) => entity.id.StartsWith("X-") && !Regex.IsMatch(entity.id, "/[0-9]/")
      )
    )
    {
      level.normalEnabled = true;
    }

    // Show all Act 3 levels before we unlock 3-1
    foreach (SelectableEntity level in __instance.selectableEntities.Where((entity) => entity.id.StartsWith("3-")))
    {
      level.normalEnabled = true;
      level.hardEnabled = true;
    }

    // Show all Act 5 levels before we clear 5-1
    foreach (SelectableEntity level in __instance.selectableEntities.Where((entity) => entity.id.StartsWith("5-")))
    {
      level.normalEnabled = true;
      if (level.id is "5-1" or "5-2" or "5-3")
        level.hardEnabled = true;
    }
    GameObject.Find("/Scene/Levels Container/Vertical Movement/5-2").SetActive(true);
    GameObject.Find("/Scene/Levels Container/Vertical Movement/5-3").SetActive(true);
    GameObject.Find("/Scene/Levels Container/Vertical Movement/5-X").SetActive(true);

    // TODO: foreach over all of Pulse's available level enum and enable them based on if they have normal/hard level.
    (SelectableEntity entity, bool? normalEnabled, bool? hardEnabled)[] toEnable =
    [
      (__instance.GetSelectableEntity("1-CNY"), true, null), // timed level
      (__instance.GetSelectableEntity("1-BOO"), true, null), // timed level
      (__instance.GetSelectableEntity("2-B1"), true, null), // bonus level
      (__instance.GetSelectableEntity("5-B1"), true, null), // bonus level
      (__instance.GetSelectableEntity("5-1"), true, true), // 5-1N before 5-1
      (__instance.GetSelectableEntity("6-1"), true, true), // record room levels before 6-1
      (__instance.GetSelectableEntity("6-X"), true, true), // record room levels before 6-1
      (__instance.GetSelectableEntity("7-1"), true, true), // record room levels before 6-1
      (__instance.GetSelectableEntity("1-X"), null, true), // 1-XN
      (__instance.GetSelectableEntity("2-X"), true, true), // 2-X before 2-4, 2-XN before 7-1
      (__instance.GetSelectableEntity("7-X"), true, true),
      (__instance.GetSelectableEntity("MD-1"), true, null), // MD-1 has additional check for 3-X
      (__instance.GetSelectableEntity("X-0"), true, null), // MD-1 has additional check for 3-X
      (__instance.GetSelectableEntity("X-1"), true, null), // MD-1 has additional check for 3-X
      (__instance.GetSelectableEntity("VoidItem1"), true, null), // abandoned ward items
      (__instance.GetSelectableEntity("VoidItem2"), true, null), // abandoned ward items
      (__instance.GetSelectableEntity("VoidItem3"), true, null), // abandoned ward items
    ];
    foreach ((SelectableEntity entity, bool? normalEnabled, bool? hardEnabled) in toEnable)
    {
      entity.normalEnabled = normalEnabled ?? entity.normalEnabled;
      entity.hardEnabled = hardEnabled ?? entity.hardEnabled;
      entity.gameObject.SetActive(true);
    }

    GameObject.Find("/Scene/Levels Container/Vertical Movement/6-2").SetActive(true);
    // Fixing Abandoned Ward items colours and unlocking 7-X2 (if 7-X was passed)
    __instance.abandonedWard.voidItems[0].color = new Color(1, 1, 1, 1);
    __instance.abandonedWard.voidItems[1].color = new Color(1, 1, 1, 1);
    __instance.abandonedWard.voidItems[2].color = new Color(1, 1, 1, 1);
    Persistence.SetLevelRank(Level.Montage, Rank.NotFinished);
    if (Persistence.GetLevelRank(Level.Montage).passed)
    {
      __instance.GetSelectableEntity("7-X2").normalEnabled = true;
      Persistence.SetLevelRank(Level.Montage2, Rank.NotFinished);
    }
    #endregion
  }

  [HarmonyPatch(nameof(scnLevelSelect.LoadLevelData))]
  [HarmonyTranspiler]
  private static IEnumerable<CodeInstruction> DoNotUnlockEntrancesPatch(IEnumerable<CodeInstruction> instructions)
  {
    CodeMatcher matcher = new(instructions);

    foreach (CodeInstruction instruction in matcher.Instructions())
    {
      if (
        instruction.opcode == OpCodes.Call
        && (MethodInfo)instruction.operand
          == AccessTools.Method(
            typeof(scnLevelSelect),
            nameof(scnLevelSelect.UnlockEntrance),
            [typeof(SelectableEntity)]
          )
      )
      {
        instruction.opcode = OpCodes.Nop;
      }
    }

    return matcher.InstructionEnumeration();
  }

  [HarmonyPatch(nameof(scnLevelSelect.UnlockNextNormalLevels))]
  [HarmonyPatch(nameof(scnLevelSelect.UnlockNightShiftLevel))]
  [HarmonyPatch(nameof(scnLevelSelect.LevelJustPassedSequenceCo))]
  [HarmonyPatch(nameof(scnLevelSelect.PlaceToPlaceTransitionCo))]
  [HarmonyPatch(nameof(scnLevelSelect.PlaceToPlaceTransitionPart2Co))]
  [HarmonyPrefix]
  private static void DoNotUnlockLevelsOrPlayCutscenesPatch(ref bool __runOriginal)
  {
    __runOriginal = false;
  }

  /// <summary>
  /// Do not unlock levels while loading level data (i.e. collabs).
  /// </summary>
  /// <param name="instructions">Original method IL instructions.</param>
  /// <returns>Modified IL instructions.</returns>
  [HarmonyPatch(nameof(scnLevelSelect.LoadLevelData))]
  [HarmonyPatch(nameof(scnLevelSelect.Awake))]
  [HarmonyTranspiler]
  private static IEnumerable<CodeInstruction> DoNotUnlockLevelsPatch(IEnumerable<CodeInstruction> instructions)
  {
    CodeMatcher matcher = new(instructions);

    // TODO: convert to using CodeMatcher.Repeat
    List<CodeInstruction> instructionList = matcher.Instructions();
    for (int i = 0; i < instructionList.Count; i++)
    {
      CodeInstruction instruction = instructionList[i];
      if (instruction.opcode == OpCodes.Call)
      {
        // Because for some reason RD uses both Level and string (calls ToString on **LEVEL**) for others
        int initialPos;
        if (
          (MethodInfo)instruction.operand
          == AccessTools.Method(
            typeof(Persistence),
            nameof(Persistence.SetLevelRank),
            [typeof(string), typeof(Rank), typeof(bool), typeof(bool)]
          )
        )
        {
          initialPos = -9;
        }
        else if (
          (MethodInfo)instruction.operand
          == AccessTools.Method(
            typeof(Persistence),
            nameof(Persistence.SetLevelRank),
            [typeof(Level), typeof(Rank), typeof(bool), typeof(bool)] // int = Level enum
          )
        )
        {
          initialPos = -5;
        }
        else
        {
          continue;
        }

        for (int pos = initialPos; pos <= 1; pos++)
        {
          instructionList[i + pos].opcode = OpCodes.Nop;
        }
      }
    }

    return matcher.InstructionEnumeration();
  }

  /// <summary>
  /// Yeet the existing logic for adding vertical destinations and inject our own.
  /// </summary>
  /// <param name="instructions">Original method IL instructions</param>
  /// <returns>Modified IL instructions</returns>
  [HarmonyPatch(nameof(scnLevelSelect.ShowRanksText))]
  [HarmonyTranspiler]
  private static IEnumerable<CodeInstruction> CustomPopulateSelectedVerticalDestinationsPatch(
    IEnumerable<CodeInstruction> instructions
  )
  {
    int match = 0;
    return new CodeMatcher(instructions)
      .MatchForward(
        true,
        new CodeMatch(
          OpCodes.Stfld,
          AccessTools.Field(typeof(scnLevelSelect), nameof(scnLevelSelect.selectedVerticalDestinations))
        )
      )
      .Repeat(codeMatcher =>
      {
        codeMatcher.Advance(1); // move past Stfld statement
        switch (match)
        {
          // BasementComputer
          case 0:
            codeMatcher.Insert(
              Transpilers.EmitDelegate<Action>(() =>
              {
                Plugin.Logger.LogInfo("Setting Ian's laptop vertical destinations");
                scnLevelSelect.instance.selectedVerticalDestinations =
                  new List<scnLevelSelect.LevelSelectDestination>();
                if (
                  // ReSharper disable once NullableWarningSuppressionIsUsed
                  Plugin.StoryClient.Session!.Items.AllItemsReceived.Any(item =>
                    item.ItemId == Bindings.SLEEVE_PAINT_ITEM_ID
                  )
                )
                {
                  Plugin.Logger.LogInfo("Adding sleevePaint");
                  scnLevelSelect.instance.selectedVerticalDestinations.Add(
                    new scnLevelSelect.LevelSelectDestination(
                      "sleevePaint",
                      RDString.Get("levelSelect.SleevePaint"),
                      RDString.Get("levelSelect.SleevePaint.details")
                    )
                  );
                }

                Plugin.Logger.LogInfo("Adding IanDesktop");
                scnLevelSelect.instance.selectedVerticalDestinations.Add(
                  new scnLevelSelect.LevelSelectDestination(
                    "IanDesktop",
                    RDString.Get("levelSelect.IanDesktop"),
                    RDString.Get($"levelSelect.IanDesktop.details.{((!Persistence.GetIanDesktopLogin()) ? 1 : 2)}")
                  )
                );
              })
            );
            break;
          // MainElevator
          case 1:
            codeMatcher.Insert(
              Transpilers.EmitDelegate<Action>(() =>
              {
                Plugin.Logger.LogInfo("Setting main elevator vertical destinations");
                scnLevelSelect.instance.selectedVerticalDestinations =
                  new List<scnLevelSelect.LevelSelectDestination>();
                if (
                  // ReSharper disable once NullableWarningSuppressionIsUsed
                  Plugin.StoryClient.Session!.Items.AllItemsReceived.Any(item =>
                    item.ItemId == Bindings.RegionToKeyID[Region.RecordsRoom]
                  )
                )
                {
                  Plugin.Logger.LogInfo($"Adding {scnLevelSelect.ExitRecordsRoom}");
                  scnLevelSelect.instance.selectedVerticalDestinations.Add(
                    new scnLevelSelect.LevelSelectDestination(
                      scnLevelSelect.ExitRecordsRoom,
                      RDString.Get("levelSelect.act6"),
                      RDString.Get("levelSelect.GoToRecordsRoom.day")
                    )
                  );
                }

                if (HasUnlockedBossSong(Act.Act7))
                {
                  Plugin.Logger.LogInfo($"Adding {scnLevelSelect.ExitVoid}");
                  scnLevelSelect.instance.selectedVerticalDestinations.Add(
                    new scnLevelSelect.LevelSelectDestination(
                      scnLevelSelect.ExitVoid,
                      RDString.Get("levelSelect.finale"),
                      RDString.Get("levelSelect.GoToVoid.day")
                    )
                  );
                }
              })
            );
            break;
          default:
            Plugin.Logger.LogWarning("Matched more than two vertical destinations - plugin may need update!!");
            break;
        }
        match++;
      })
      .Start()
      .MatchForward(
        true,
        new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)48),
        new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(RDUtils), nameof(RDUtils.Passed))),
        new CodeMatch(OpCodes.Brfalse)
      )
      .ThrowIfInvalid("Couldn't find Level.Bitterness.Passed()")
      .InsertAndAdvance(new CodeInstruction(OpCodes.Pop)) // consume the bool on stack
      .SetOpcodeAndAdvance(OpCodes.Br) // replacing Brfalse, but keeping operand
      .InstructionEnumeration();
  }

  [HarmonyPatch(nameof(scnLevelSelect.UpdateCharacters))]
  [HarmonyPostfix]
  private static void UpdateCharactersPatch(scnLevelSelect __instance)
  {
    // Fix Rin's shader on her RunningCharacter
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

      if (shader != null)
      {
        GameObject rinObject = GameObject.Find("/Scene/Corridor/GoToMuseDashRoom/Rin");
        scrChar rinChar = rinObject.GetComponent<scrChar>();
        rinChar.shaderData = shader;
        shader.SetFrameChanged();
      }
    }

    // Remove the tarp covering the Rhythm Weightlifter cab
    if (
      Persistence.GetLevelRank(Level.RhythmWeightlifter) != Rank.NotAvailable
      && __instance.currentDifficulty != Difficulty.Hard
    )
    {
      // Near the end of scnLevelSelect.UpdateCharacters():
      // TODO: Consider using reverse transpiler
      __instance.rwCabinetTarp.SetActive(false);
      __instance.rwCabinet.gameObject.SetActive(true);
      __instance.rwLowpassFilter.cutoffFrequency = 2800f;
    }
  }

  internal static void TryUnlockAllBossSongs()
  {
    Plugin.Logger.LogInfo($"[{nameof(UnlockItemPatch)}] Attempting to unlock all boss songs");
    foreach (Act act in Enum.GetValues(typeof(Act)))
    {
      TryUnlockBossSong(act);
    }
  }

  internal static bool TryUnlockBossSong(Act act)
  {
    Plugin.Logger.LogDebug($"[{nameof(UnlockItemPatch)}] Attempting to unlock {act}'s boss songs");
    if (HasUnlockedBossSong(act))
    {
      Level[] levelBosses = Bindings.ActBoss[act];
      Plugin.Logger.LogDebug($"[{nameof(UnlockItemPatch)}] Unlocked {act}'s boss song(s) [{levelBosses.Join()}]");

      foreach (Level levelBoss in levelBosses)
      {
        Persistence.SetLevelRank(levelBoss, Rank.NotFinished);
      }

      return true;
    }
    Plugin.Logger.LogDebug($"[{nameof(UnlockItemPatch)}] Does not meet requirements to unlock {act}'s boss song");
    return false;
  }

  private static bool HasUnlockedBossSong(Act act)
  {
    if (act == Act.None)
    {
      return false;
    }

    Plugin.Logger.LogDebug($"Checking act {act}");
    int clearedInAct = 0;
    foreach (Level level in Bindings.LevelsInAct[act])
    {
      if (Bindings.LevelsThatDoNotUnlockBoss.Contains(level))
      {
        Plugin.Logger.LogDebug($"Level {level} is marked as does not unlock boss, skipping");
        continue;
      }

      int minimumRank = Plugin.StoryClient.Slot.bossUnlockRequirement switch
      {
        SlotData.BossUnlockRequirement.ARankAll => Rank.A,
        SlotData.BossUnlockRequirement.Perfect => Rank.S,
        SlotData.BossUnlockRequirement.BRankAll => Rank.B,
        _ => throw new IndexOutOfRangeException("Boss unlock requirement out of valid range"),
      };

      Plugin.Logger.LogDebug($"Checking level {level}");
      Rank rank = Persistence.GetLevelRank(level);
      if (rank.ToNormal() >= minimumRank)
      {
        clearedInAct++;

        long clearRequirement = Plugin.StoryClient.Slot.GetBossSongLevelClearRequirement(act);
        if (clearedInAct >= clearRequirement)
        {
          Plugin.Logger.LogInfo($"Unlocking {act} boss ({clearRequirement} requirement, rank {minimumRank})");
          return true;
        }
      }
    }

    return false;
  }
}
