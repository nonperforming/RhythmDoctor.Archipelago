namespace RhythmDoctor.Archipelago.World.Structures;

// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnassignedField.Global
public struct ItemsData
{
  [YamlMember(Alias = "levels")]
  public Dictionary<Ward, Dictionary<LevelStage, Item>> Levels;

  [YamlMember(Alias = "keys")]
  public Dictionary<Ward, Item> Keys;

  [YamlMember(Alias = "filler")]
  public Dictionary<FillerType, List<Item>> Filler;
}
// ReSharper restore CollectionNeverUpdated.Global
// ReSharper restore UnassignedField.Global
