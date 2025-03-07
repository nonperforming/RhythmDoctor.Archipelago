namespace RhythmDoctor.Archipelago.World;

internal class Items
{
  private ItemsData _data;

  public Items()
  {
    _data = new();
  }

  private Region GetWardFromLevelItem(long id)
  {
    Region region;
    if (id < _data.Levels[Region.MainWard].Count)
    {
      region = Region.MainWard;
    }
    else if (id < _data.Levels[Region.SVTWard].Count)
    {
      region = Region.SVTWard;
    }
    else if (id < _data.Levels[Region.Train].Count)
    {
      region = Region.Train;
    }
    else if (id < _data.Levels[Region.PhysiotherapyWard].Count)
    {
      region = Region.PhysiotherapyWard;
    }
    else if (id < _data.Levels[Region.Basement].Count)
    {
      region = Region.Basement;
    }
    else if (id < _data.Levels[Region.ArtRoom].Count)
    {
      region = Region.ArtRoom;
    }
    else
    {
      throw new ArgumentOutOfRangeException($"{id} is out of range of item IDs");
    }

    return region;
  }

  internal Item GetLevelItem(LevelStage level)
  {
    foreach (Dictionary<LevelStage, Item>? items in _data.Levels.Values)
    {
      if (items.TryGetValue(level, out Item item))
      {
        return item;
      }
    }
    throw new ArgumentOutOfRangeException($"Couldn't find {level}'s item");
  }

  internal bool HaveItem(Item item) => HaveItem(item.ID);

  internal bool HaveItem(long id)
  {
    if (Plugin.Client == null || Plugin.Client.session == null)
    {
      throw new NullReferenceException("Client/sesion is not initialized");
    }

    foreach (ItemInfo item in Plugin.Client.session.Items.AllItemsReceived)
    {
      if (id == item.ItemId)
      {
        return true;
      }
    }
    return false;

    // // FIXME: This is incredibly inefficient!!
    // foreach (KeyValuePair<Ward, Dictionary<LevelStage, Item>> levels in _data.Levels)
    // {
    //   foreach (KeyValuePair<LevelStage, Item> level in levels.Value)
    //   {
    //     if (id == level.Value.ID)
    //     {
    //       return true;
    //     }
    //   }
    // }
    //
    // foreach (KeyValuePair<Ward, Item> item in _data.Keys)
    // {
    //   if (id == item.Value.ID)
    //   {
    //     return true;
    //   }
    // }
    //
    // foreach (KeyValuePair<FillerType, List<Item>> items in _data.Filler)
    // {
    //   foreach (Item item in items.Value)
    //   {
    //     if (id == item.ID)
    //     {
    //       return true;
    //     }
    //   }
    // }
  }

  internal Item GetItem(long id)
  {
    Region region = GetWardFromLevelItem(id);

    foreach (Item item in _data.Levels[region].Values)
    {
      if (item.ID == id)
      {
        return item;
      }
    }

    throw new ArgumentOutOfRangeException($"Couldn't find {id} in item IDs");
  }

  internal bool IsLevelItem(ReceivedItemsHelper helper) => IsLevelItem(helper.PeekItem());

  internal bool IsLevelItem(ItemInfo data) => IsLevelItem(data.ItemId);

  internal bool IsLevelItem(long id)
  {
    Region region = GetWardFromLevelItem(id);

    foreach (KeyValuePair<LevelStage, Item> key in _data.Levels[region])
    {
      if (key.Value.ID == id)
      {
        return true;
      }
    }
    return false;
  }

  internal bool IsKeyItem(ReceivedItemsHelper helper) => IsKeyItem(helper.PeekItem());

  internal bool IsKeyItem(ItemInfo data) => IsKeyItem(data.ItemId);

  internal bool IsKeyItem(long id)
  {
    foreach (KeyValuePair<Region, Item> key in _data.Keys)
    {
      if (key.Value.ID == id)
        return true;
    }
    return false;
  }

  internal Region? GetKeyItem(long id)
  {
    foreach (KeyValuePair<Region, Item> key in _data.Keys)
    {
      if (key.Value.ID == id)
        return key.Key;
    }
    return null;
  }

  /// <summary>
  /// Gets the LevelStage of an item
  /// </summary>
  /// <returns></returns>
  internal LevelStage? GetLevelStageFromItem(long id)
  {
    Region region = GetWardFromLevelItem(id);

    foreach ((LevelStage levelStage, Item item) in _data.Levels[region])
    {
      if (id == item.ID)
      {
        return levelStage;
      }
    }

    return null;
  }

  internal bool IsTrapItem(ReceivedItemsHelper helper) => IsTrapItem(helper.PeekItem());
  internal bool IsTrapItem(ItemInfo data) => IsTrapItem(data.ItemId);

  internal bool IsTrapItem(long id)
  {
    foreach (Item key in _data.Filler[FillerType.Traps])
    {
      if (key.ID == id)
        return true;
    }
    return false;
  }
}
