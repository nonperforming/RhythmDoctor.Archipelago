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
#pragma warning disable CS8602 // Dereference of a possibly null reference.
    foreach (ItemInfo item in Plugin.Client.Session.Items.AllItemsReceived)
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    {
      if (Bindings.KeyItemIdToWard.TryGetValue(item.ItemId, out Region region))
      {
        Plugin.Logger.LogInfo($"Unlocking entrance {region}");
        scnLevelSelect.instance.UnlockEntrance(region);
      }

      if (!sleevePaint && item.ItemId == Bindings.SLEEVE_PAINT_ITEM_ID)
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
      Plugin.Logger.LogDebug($"Checking level {level}");
      Rank rank = Persistence.GetLevelRank(level);
      if (rank.passed)
      {
        clearedInAct++;
        if (clearedInAct >= Bindings.ClearedLevelsToUnlockBoss[act])
        {
          Plugin.Logger.LogInfo($"Unlocking {act} boss");
          return true;
        }
      }
    }

    return false;
  }
}
