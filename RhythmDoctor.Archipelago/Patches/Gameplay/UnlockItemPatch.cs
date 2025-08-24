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

    // TODO: Reimplement with new system
    // foreach (ItemInfo itemInfo in Plugin.Client.session?.Items.AllItemsReceived!)
    // {
    //   if (Plugin.Client.items.IsLevelItem(itemInfo.ItemId))
    //   {
    //     LevelStage? levelStage = Plugin.Client.items.GetLevelStageFromItem(itemInfo.ItemId);
    //
    //     if (levelStage == null)
    //     {
    //       Plugin.Logger.LogError($"Couldn't find LevelStage of item {itemInfo.ItemId}");
    //       continue;
    //     }
    //
    //     string? levelName = levelStage.Value.GetEnumMember();
    //     if (levelName == null)
    //       continue;
    //
    //     SelectableEntity selectableEntity = scnLevelSelect.instance.FindSelectableEntity(levelName);
    //     // SelectableCharacter selectableCharacter = (SelectableCharacter)selectableEntity; // What do we need this for?
    //
    //     selectableEntity.gameObject.SetActive(true);
    //     // selectableCharacter.normalEnabled = true;
    //     // selectableCharacter.hardEnabled = true;
    //     selectableEntity.normalEnabled = true;
    //     selectableEntity.hardEnabled = true;
    //   }
    //   else if (Plugin.Client.items.IsKeyItem(itemInfo.ItemId))
    //   {
    //     Region? regionToUnlock = Plugin.Client.items.GetKeyItem(itemInfo.ItemId);
    //
    //     if (regionToUnlock == null)
    //     {
    //       Plugin.Logger.LogError($"Couldn't find Region of item {itemInfo.ItemId}");
    //       continue;
    //     }
    //
    //     LevelHelper.UnlockEntrance(regionToUnlock.Value);
    //   }
    // }
  }
}
