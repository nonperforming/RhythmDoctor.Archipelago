#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
namespace RhythmDoctor.Archipelago.World.Structures;

// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnassignedField.Global
internal struct ItemsData
{
  [YamlMember(Alias = "levels")]
  internal Dictionary<Region, Dictionary<LevelStage, Item>> Levels;

  [YamlMember(Alias = "keys")]
  internal Dictionary<Region, Item> Keys;

  [YamlMember(Alias = "filler")]
  internal Dictionary<FillerType, List<Item>> Filler;
}
// ReSharper restore CollectionNeverUpdated.Global
// ReSharper restore UnassignedField.Global
