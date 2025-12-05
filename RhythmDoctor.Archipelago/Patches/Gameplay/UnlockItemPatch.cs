using System.Reflection;

namespace RhythmDoctor.Archipelago.Patches.Gameplay;

[HarmonyPatch(typeof(scnLevelSelect))]
internal static class UnlockItemPatch
{
  [HarmonyPatch(nameof(scnLevelSelect.LoadLevelData))]
  [HarmonyPostfix]
  private static void UnlockEntitiesWithItemsPatch(scnLevelSelect __instance)
  {
    // This is a PostLogin patch, session is guaranteed to exist (assuming going through normal flow)
    Plugin.Logger.LogInfo("Checking for extra unlocks");

    // Unlocking regions and Sleeve Paint
    Plugin.Logger.LogInfo("Checking for regions to unlock");

    bool sleevePaint = false;
    // ReSharper disable once NullableWarningSuppressionIsUsed
    foreach (ItemInfo item in Plugin.Client.Session!.Items.AllItemsReceived)
    {
      if (Bindings.KeyItemIdToWard.TryGetValue(item.ItemId, out Region region))
      {
        Plugin.Logger.LogInfo($"Unlocking entrance {region}");
        scnLevelSelect.instance.UnlockEntrance(region);
      }

      if (item.ItemId == Bindings.SLEEVE_PAINT_ITEM_ID)
      {
        sleevePaint = true;
      }
    }

    // Sleeve Paint is unlocked by default.
    if (!sleevePaint)
    {
      Plugin.Logger.LogInfo("Locking Sleeve Paint");
      SelectableEntity sleevePaintEntity = __instance.FindSelectableEntity("SleevePaintComputer");
      sleevePaintEntity.normalEnabled = sleevePaintEntity.hardEnabled = false;
      GameObject.Find("/Scene/Corridor/SleevePaintComputer").gameObject.SetActive(false);
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
      artExercise.group = 3; // Basement group
      // We need to move this to the left-most level in the Basement.
      // Currently, this is the vivid/stasis collab X-FTS.
      // However, this may change in the future when more collabs take place.
      // In selectableEntities, the unreleased level 'FlyAway' is directly above X-FTS
      //  and is probably a good place to insert X-1 under,
      //  seeing as this level is still here as of the Steam release (2021)
      int index = __instance.selectableEntities.FindIndex((entity) => entity.gameObject.name == "FlyAway") + 1;
      artExercise.gamePosition = new Vector2(2535, 56); // a little bit left to the fireplace/boiler in Ian's office
      __instance.selectableEntities.Remove(artExercise);
      __instance.selectableEntities.Insert(index, artExercise);

      // Unlock the Art Room if we have all bosses completed.
      if (Bindings.ActBoss.Values.All(level => Persistence.GetLevelRank(level).passed))
      {
        Plugin.Logger.LogInfo("Unlocking Art Room and X-0 - Helping Hands");
        __instance.UnlockEntrance(__instance.FindSelectableEntity("GoToArtRoom"));
        Persistence.SetLevelRank(Level.HelpingHands, Rank.NotFinished, false, false);
      }
    }

    // Hiding Paige at the vending machine
    // Even though we are skipping cutscenes for some reason Paige can show up at the vending machine
    GameObject.Find("/Scene/Corridor/VendingMachinePaige").gameObject.SetActive(false);

    // Unhiding levels
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
      if (level.id is "5-1" or "5-2")
        level.hardEnabled = true;
    }
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
    // Unhiding 2-X
    __instance.GetSelectableEntity("2-X").normalEnabled = true;
    // Unhiding 5-1N before we pass 5-1 (normally when we are out of dream)
    SelectableEntity FiveOne = __instance.GetSelectableEntity("5-1");
    FiveOne.normalEnabled = true;
    FiveOne.hardEnabled = true;
    // Unhiding X-0 before we pass the last released boss song
    SelectableEntity X0 = __instance.GetSelectableEntity("X-0");
    X0.normalEnabled = true;
    X0.gameObject.SetActive(true);
    // Unhiding X-1 before we pass the last released boss song
    SelectableEntity X1 = __instance.GetSelectableEntity("X-1");
    X1.normalEnabled = true;
    X1.gameObject.SetActive(true);

    // Unlock the boss if enough levels in its act has been completed
    foreach (Act act in Enum.GetValues(typeof(Act)))
    {
      if (HasUnlockedBossSong(act))
      {
        Persistence.SetLevelRank(Bindings.ActBoss[act], Rank.NotFinished, false, false);
      }
    }
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
  [HarmonyTranspiler]
  private static IEnumerable<CodeInstruction> DoNotUnlockLevelsPatch(IEnumerable<CodeInstruction> instructions)
  {
    CodeMatcher matcher = new(instructions);
    // csharpier-ignore
    int startIndex = matcher
      .MatchForward(false, new CodeMatch(OpCodes.Ldstr, "1-X"))
      .Advance(-2)
      .Pos;
    int endIndex = matcher
      .MatchForward(false, new CodeMatch(OpCodes.Newobj, AccessTools.Constructor(typeof(List<Level>))))
      .Advance(-1)
      .Pos;

    matcher.RemoveInstructionsInRange(startIndex, endIndex);
    return matcher.InstructionEnumeration();
  }

  [HarmonyPatch(nameof(scnLevelSelect.Awake))]
  [HarmonyTranspiler]
  private static IEnumerable<CodeInstruction> DoNotUnlockArtRoomLevels(IEnumerable<CodeInstruction> instructions)
  {
    // csharpier-ignore
    return new CodeMatcher(instructions)
      // For Helping Hands
      .MatchForward(false, new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(RDUtils), nameof(RDUtils.Locked))))
      .Advance(-1).SetOpcodeAndAdvance(OpCodes.Nop) // otherwise stack will be messed up
      .SetOpcodeAndAdvance(OpCodes.Ldc_I4_0) // force false
      // For Art Exercise
      .MatchForward(false, new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(RDUtils), nameof(RDUtils.Locked))))
      .Advance(-1).SetOpcodeAndAdvance(OpCodes.Nop) // otherwise stack will be messed up
      .SetOpcodeAndAdvance(OpCodes.Ldc_I4_0) // force false
      .InstructionEnumeration();
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

      int minimumRank;
      switch (Plugin.Client.Slot.bossUnlockRequirement)
      {
        case SlotData.BossUnlockRequirement.ARankAll:
          minimumRank = Rank.A;
          break;
        case SlotData.BossUnlockRequirement.Perfect:
          minimumRank = Rank.S;
          break;
        case SlotData.BossUnlockRequirement.BRankAll:
        case SlotData.BossUnlockRequirement.Half:
          minimumRank = Rank.B;
          break;
        default:
          throw new IndexOutOfRangeException("Boss unlock requirement out of valid range");
      }

      Plugin.Logger.LogDebug($"Checking level {level}");
      Rank rank = Persistence.GetLevelRank(level);
      if (rank.ToNormal() >= minimumRank)
      {
        clearedInAct++;

        if (Plugin.Client.Slot.bossUnlockRequirement == SlotData.BossUnlockRequirement.Half)
        {
          // result truncates towards 0 - both are guaranteed to be positive so result is equivalent to floor
          int levelsToClear = Bindings.LevelCountInActUnlockingBoss[act] / 2;
          if (clearedInAct >= levelsToClear)
          {
            Plugin.Logger.LogInfo($"Unlocking {act} boss (half requirement, {levelsToClear} levels to clear)");
            return true;
          }
        }
        else if (clearedInAct >= Bindings.LevelCountInActUnlockingBoss[act])
        {
          Plugin.Logger.LogInfo($"Unlocking {act} boss (full requirement, rank {minimumRank})");
          return true;
        }
      }
    }

    return false;
  }
}
