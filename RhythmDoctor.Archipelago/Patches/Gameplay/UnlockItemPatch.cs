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
