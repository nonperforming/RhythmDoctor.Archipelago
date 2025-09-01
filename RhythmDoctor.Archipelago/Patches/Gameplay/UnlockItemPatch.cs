namespace RhythmDoctor.Archipelago.Patches.Gameplay;

[HarmonyPatch(typeof(scnLevelSelect))]
static class UnlockItemPatch
{
  [HarmonyPatch(nameof(scnLevelSelect.LoadLevelData))]
  [HarmonyPostfix]
  static void UnlockEntitiesWithItemsPatch(scnLevelSelect __instance)
  {
    // This is a PostLogin patch, session is guaranteed to exist (assuming going through normal flow)
    Plugin.Logger.LogInfo("Checking for extra unlocks");

    // Unlocking regions
    Plugin.Logger.LogInfo("Checking for regions to unlock");
#pragma warning disable CS8602 // Dereference of a possibly null reference.
    foreach (ItemInfo item in Plugin.Client.session.Items.AllItemsReceived)
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    {
      if (Bindings.KeyItemIdToWard.TryGetValue(item.ItemId, out Region region))
      {
        Plugin.Logger.LogInfo($"Unlocking entrance {region}");
        LevelHelper.UnlockEntrance(region);
      }
    }

    // Moving 1-CNY.
    // If we do not do this, 1-CNY and 1-BOO will overlap each other.
    Plugin.Logger.LogInfo("Moving 1-CNY");
    __instance.FindSelectableEntity("1-CNY").gamePosition.x = -564;

    if (Plugin.Client.slotData.endGoal == SlotData.EndGoal.HelpingHands)
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
      artExercise.gamePosition = new Vector2(2489, 56);
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

    // Unhiding Act 3 levels
    foreach (SelectableEntity level in __instance.selectableEntities.Where((entity) => entity.id.StartsWith("3-")))
    {
      level.normalEnabled = true;
      level.hardEnabled = true;
    }

    // Unlock the boss if enough levels in its act has been completed
    Plugin.Logger.LogInfo("Checking if boss is available");
    foreach (Act act in Enum.GetValues(typeof(Act)))
    {
      if (act == Act.None)
      {
        continue;
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
            Persistence.SetLevelRank(Bindings.ActBoss[act], Rank.NotFinished, false, false);
            break;
          }
        }
      }
    }
  }
}
