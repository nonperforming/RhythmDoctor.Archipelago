using System.Reflection;

namespace RhythmDoctor.Archipelago.Patches.Gameplay;

using System.Text.RegularExpressions;

[HarmonyPatch(typeof(scnLevelSelect))]
internal static class UnlockItemPatch
{
  [HarmonyPatch(nameof(scnLevelSelect.LoadLevelData))]
  [HarmonyPostfix]
  private static void UnlockEntitiesWithItemsPatch(scnLevelSelect __instance)
  {
    // TODO: Bit inefficient

    // This is a PostLogin patch, session is guaranteed to exist (assuming going through normal flow)
    Plugin.Logger.LogInfo("Checking for extra unlocks");

    // Unlocking regions and Sleeve Paint
    Plugin.Logger.LogInfo("Checking for regions to unlock");

    bool hasBasementKey = false;
    // ReSharper disable once NullableWarningSuppressionIsUsed
    foreach (ItemInfo item in Plugin.Client.Session!.Items.AllItemsReceived)
    {
      Plugin.Logger.LogDebug($"Processing item {item.ItemName} ({item.ItemId})");
      if (Bindings.KeyItemIdToWard.TryGetValue(item.ItemId, out Region region))
      {
        if (region == Region.Basement)
        {
          hasBasementKey = true;
        }
        Plugin.Logger.LogInfo($"Unlocking entrance {region}");
        scnLevelSelect.instance.UnlockEntrance(region);
      }
      else if (Bindings.ItemIdToLevel.TryGetValue(item.ItemId, out Level level))
      {
        // Duplicated in Client, though sometimes it can appear to "drop" items (possibly issue with
        //  being in a loading state/between scenes) which this should catch
        Plugin.Logger.LogInfo(
          $"[{nameof(UnlockItemPatch)}] Unlocking stage item {item.ItemName} ({item.ItemId}, {level})"
        );
        Persistence.SetLevelRank(level, Rank.NotFinished, false, false);
      }
    }

    if (!hasBasementKey)
    {
      Plugin.Logger.LogInfo("Locking basement");
      __instance.LockEntrance(scnLevelSelect.GoToBasement);
    }

    // Moving 1-CNY.
    // If we do not do this, 1-CNY and 1-BOO will overlap each other.
    Plugin.Logger.LogInfo("Moving 1-CNY");
    __instance.FindSelectableEntity("1-CNY").gamePosition.x = -564;

    if (Plugin.Client.Slot.endGoal == SlotData.EndGoal.HelpingHands)
    {
      // Moving X-1 - Art Exercise to the basement if end goal is X-0 - Helping Hands
      Plugin.Logger.LogInfo("Moving X-1 to the basement");
      SelectableEntity artExercise = __instance.FindSelectableEntity("X-1");
      // Moving from group 6 (Art Room) to 3 (Basement)
      artExercise.group = scnLevelSelectExtensions.BASEMENT_AREA;
      // Set selection order to just after the basement computer. This is separate from world position.
      int index =
        __instance.selectableEntities.FindIndex((entity) => entity.gameObject.name == scnLevelSelect.BasementComputer)
        + 1;
      artExercise.gamePosition = new Vector2(2480, 53); // in front of Ian's bookshelf/archive thing
      __instance.selectableEntities.Remove(artExercise);
      __instance.selectableEntities.Insert(index, artExercise);

      // Unlock the Art Room if we have all bosses completed.
      if (Bindings.ActBoss.Values.All(levels => levels.All(level => Persistence.GetLevelRank(level).passed)))
      {
        Plugin.Logger.LogInfo("Unlocking Art Room and X-0 - Helping Hands");
        __instance.UnlockEntrance(__instance.FindSelectableEntity(scnLevelSelect.GoToArtRoom));
        Persistence.SetLevelRank(Level.HelpingHands, Rank.NotFinished, false, false);
      }
    }

    // Hiding Paige at the vending machine
    // Even though we are skipping cutscenes for some reason Paige can show up at the vending machine
    GameObject.Find("/Scene/Corridor/VendingMachinePaige").gameObject.SetActive(false);

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

    // Show all bonus levels/make them selectable
    __instance.GetSelectableEntity("2-B1").normalEnabled = true;
    __instance.GetSelectableEntity("5-B1").normalEnabled = true;

    // Show all Act 3 levels before we unlock 3-1
    foreach (SelectableEntity level in __instance.selectableEntities.Where((entity) => entity.id.StartsWith("3-")))
    {
      level.normalEnabled = true;
      if (level.id != "3-X")
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
    // Unhiding timed levels 1-CNY and 1-BOO
    SelectableEntity CNY = __instance.GetSelectableEntity("1-CNY");
    CNY.normalEnabled = true;
    CNY.gameObject.SetActive(true);
    SelectableEntity BOO = __instance.GetSelectableEntity("1-BOO");
    BOO.normalEnabled = true;
    BOO.gameObject.SetActive(true);
    // Unhiding MD-1 - for some reason MD-1 has an additional check for 3-X
    __instance.GetSelectableEntity("MD-1").normalEnabled = true;
    // Unhiding 1-XN
    __instance.GetSelectableEntity("1-X").hardEnabled = true;
    // Unhiding 2-X before we pass Song of the Sea
    SelectableEntity TwoX = __instance.GetSelectableEntity("2-X");
    TwoX.normalEnabled = true;
    // Unhiding 2-XN before we pass Blurred
    TwoX.hardEnabled = true;
    // Unhiding 5-1N before we pass 5-1 (normally when we are out of dream)
    SelectableEntity FiveOne = __instance.GetSelectableEntity("5-1");
    FiveOne.normalEnabled = true;
    FiveOne.hardEnabled = true;
    // Unhiding Records Room levels before we pass 6-1
    SelectableEntity[] recordRoomLevels =
    [
      __instance.FindSelectableEntity("6-2"),
      __instance.FindSelectableEntity("6-X"),
      __instance.FindSelectableEntity("7-1"),
    ];
    foreach (SelectableEntity recordRoomLevel in recordRoomLevels)
    {
      recordRoomLevel.normalEnabled = true;
    }
    GameObject.Find("/Scene/Levels Container/Vertical Movement/6-2").SetActive(true);
    // Unhiding Abandoned Ward items and 7-X, 7-X2 (if 7-X was passed)
    __instance.GetSelectableEntity("VoidItem1").normalEnabled = true;
    __instance.GetSelectableEntity("VoidItem2").normalEnabled = true;
    __instance.GetSelectableEntity("VoidItem3").normalEnabled = true;
    __instance.abandonedWard.voidItems[0].color = new Color(1, 1, 1, 1);
    __instance.abandonedWard.voidItems[1].color = new Color(1, 1, 1, 1);
    __instance.abandonedWard.voidItems[2].color = new Color(1, 1, 1, 1);
    __instance.GetSelectableEntity("7-X").normalEnabled = true;
    Persistence.SetLevelRank(Level.Montage, Rank.NotFinished);
    if (Persistence.GetLevelRank(Level.Montage).passed)
    {
      __instance.GetSelectableEntity("7-X2").normalEnabled = true;
      Persistence.SetLevelRank(Level.Montage2, Rank.NotFinished);
    }
    // Unhiding X-0 before we pass the last released boss song
    SelectableEntity X0 = __instance.GetSelectableEntity("X-0");
    X0.normalEnabled = true;
    X0.gameObject.SetActive(true);
    // Unhiding X-1 before we pass the last released boss song
    SelectableEntity X1 = __instance.GetSelectableEntity("X-1");
    X1.normalEnabled = true;
    X1.gameObject.SetActive(true);
    #endregion

    // Unlock the boss if enough levels in its act has been completed
    foreach (Act act in Enum.GetValues(typeof(Act)))
    {
      if (HasUnlockedBossSong(act))
      {
        Plugin.Logger.LogDebug($"Unlocking act {act}");
        foreach (Level level in Bindings.ActBoss[act])
        {
          Persistence.SetLevelRank(level, Rank.NotFinished, false, false);

          if (level == Level.Montage)
          {
            __instance.UnlockEntrance(Region.RecordsRoom); // elevator
          }
        }
      }
    }
  }

  [HarmonyPatch(nameof(scnLevelSelect.LoadLevelData))]
  [HarmonyPatch(nameof(scnLevelSelect.LevelJustPassedSequenceCo))]
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

  [HarmonyPatch(nameof(scnLevelSelect.UnlockNightShiftLevel))]
  [HarmonyPrefix]
  private static void DoNotUnlockNightShiftLevelPatch(ref bool __runOriginal)
  {
    __runOriginal = false;
  }

  /// <summary>
  /// Do not unlock levels while loading level data (i.e. collabs).
  /// </summary>
  /// <param name="instructions">Original method IL instructions</param>
  /// <returns>Modified IL instructions</returns>
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
                  Plugin.Client.Session!.Items.AllItemsReceived.Any(item =>
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
                  Plugin.Client.Session!.Items.AllItemsReceived.Any(item =>
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

  internal static bool HasUnlockedBossSong(Act act)
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

      int minimumRank = Plugin.Client.Slot.bossUnlockRequirement switch
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

        long clearRequirement = Plugin.Client.Slot.GetBossSongLevelClearRequirement(act);
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
