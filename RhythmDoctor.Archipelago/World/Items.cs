namespace RhythmDoctor.Archipelago.World;

internal class Items
{
  private ItemsData _data;

  public Items()
  {
    _data = new();
  }

  private Ward GetWardFromLevelItem(long id)
  {
    Ward ward;
    if (id < _data.Levels[Ward.MainWard].Count)
    {
      ward = Ward.MainWard;
    }
    else if (id < _data.Levels[Ward.SVTWard].Count)
    {
      ward = Ward.SVTWard;
    }
    else if (id < _data.Levels[Ward.Train].Count)
    {
      ward = Ward.Train;
    }
    else if (id < _data.Levels[Ward.PhysiotherapyWard].Count)
    {
      ward = Ward.PhysiotherapyWard;
    }
    else if (id < _data.Levels[Ward.Basement].Count)
    {
      ward = Ward.Basement;
    }
    else if (id < _data.Levels[Ward.ArtRoom].Count)
    {
      ward = Ward.ArtRoom;
    }
    else
    {
      throw new ArgumentOutOfRangeException($"{id} is out of range of item IDs");
    }

    return ward;
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
    Ward ward = GetWardFromLevelItem(id);

    foreach (Item item in _data.Levels[ward].Values)
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
    Ward ward = GetWardFromLevelItem(id);

    foreach (KeyValuePair<LevelStage, Item> key in _data.Levels[ward])
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
    foreach (KeyValuePair<Ward, Item> key in _data.Keys)
    {
      if (key.Value.ID == id)
        return true;
    }
    return false;
  }

  internal void TrapItem()
  {
    // TODO
    throw new NotImplementedException();
  }
}
