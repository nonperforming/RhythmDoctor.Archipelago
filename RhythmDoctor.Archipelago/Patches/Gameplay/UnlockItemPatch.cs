namespace RhythmDoctor.Archipelago.Patches.Gameplay;

[HarmonyPatch(typeof(scnLevelSelect))]
public class UnlockItemPatch
{
  public static bool UpdateUnlockedItems = false;

  [HarmonyPatch(nameof(scnLevelSelect.LoadLevelData))]
  [HarmonyPostfix]
  static void UnlockEntitiesWithItems(ref scnLevelSelect __instance)
  {
    // TODO: We should prevent entrances and levels from being unlocked in the first place.
    // foreach (Region region in Enum.GetValues(typeof(Region)))
    // {
    //   LevelHelper.LockEntrance(region);
    // }
    //
    // Dictionary<string, object> levels = (
    //     // ReSharper disable once Unity.UnknownResource
    //     Json.Deserialize(Resources.Load<TextAsset>("levelSequence").text) as Dictionary<string, object>
    //     // ReSharper disable once NullableWarningSuppressionIsUsed
    // )!;
    // foreach (KeyValuePair<string, object> item in levels)
    // {
    //   // ReSharper disable once NullableWarningSuppressionIsUsed
    //   Dictionary<string, object> dictionary = (item.Value as Dictionary<string, object>)!;
    //   // ReSharper disable once NullableWarningSuppressionIsUsed
    //   string type = (dictionary["type"] as string)!;
    //
    //   if (type != "character")
    //     return;
    //
    //   // SelectableCharacter selectableCharacter = new SelectableCharacter();
    //   // selectableCharacter.normalEnabled = false;
    //   // selectableCharacter.hardEnabled = false;
    //   Persistence.SetLevelRank(item.Key, Rank.NotAvailable, force: false);
    // }

    // FIXME: Check if 1-CNY's setting rank to -1 will forcefully unlock it!
    // ReSharper disable once NullableWarningSuppressionIsUsed
    foreach (ItemInfo itemInfo in Plugin.Client.session?.Items.AllItemsReceived!)
    {
      if (Plugin.Client.items.IsLevelItem(itemInfo.ItemId))
      {
        LevelStage? levelStage = Plugin.Client.items.GetLevelStageFromItem(itemInfo.ItemId);

        if (levelStage == null)
        {
          Plugin.Logger.LogError($"Couldn't find LevelStage of item {itemInfo.ItemId}");
          continue;
        }

        string? levelName = levelStage.Value.GetEnumMember();
        if (levelName == null)
          continue;

        SelectableEntity selectableEntity = scnLevelSelect.instance.FindSelectableEntity(levelName);
        // SelectableCharacter selectableCharacter = (SelectableCharacter)selectableEntity; // What do we need this for?

        selectableEntity.gameObject.SetActive(true);
        // selectableCharacter.normalEnabled = true;
        // selectableCharacter.hardEnabled = true;
        selectableEntity.normalEnabled = true;
        selectableEntity.hardEnabled = true;
      }
      else if (Plugin.Client.items.IsKeyItem(itemInfo.ItemId))
      {
        Region? regionToUnlock = Plugin.Client.items.GetKeyItem(itemInfo.ItemId);

        if (regionToUnlock == null)
        {
          Plugin.Logger.LogError($"Couldn't find Region of item {itemInfo.ItemId}");
          continue;
        }

        LevelHelper.UnlockEntrance(regionToUnlock.Value);
      }
    }
  }
}
