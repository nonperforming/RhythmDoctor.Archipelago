namespace RhythmDoctor.Archipelago.World.Structures;

public struct ItemsData
{
  public ItemsData()
  {
    Plugin.Logger?.LogInfo("Creating ItemsData");
    this = DataHelper.GetItemsData();
  }

  [YamlMember(Alias = "levels")]
  public Dictionary<Ward, List<Item>> Levels;

  [YamlMember(Alias = "keys")]
  public List<Item> Keys;

  [YamlMember(Alias = "filler")]
  public Dictionary<FillerType, List<Item>> Filler;
}
