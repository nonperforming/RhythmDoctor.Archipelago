using System.Runtime.Serialization;

namespace RhythmDoctor.Archipelago.World;

public struct ItemsData
{
  [YamlMember(Alias = "levels")]
  public Dictionary<Ward, List<Item>> Levels;
  [YamlMember(Alias = "keys")]
  public List<Item> Keys;
  [YamlMember(Alias = "filler")]
  public Dictionary<FillerType, List<Item>> Filler;
}

public struct Item
{
  [YamlMember(Alias = "name")]
  public string Name;
  [YamlMember(Alias = "id")]
  public ulong ID;
  [YamlMember(Alias = "classification")]
  public ItemClassification Classification;
}

public enum Ward
{
  [EnumMember(Value = "main-ward")]
  MainWard,
  [EnumMember(Value = "svt-ward")]
  SVTWard,
  [EnumMember(Value = "train")]
  Train,
  [EnumMember(Value = "physiotherapy-ward")]
  PhysiotherapyWard,
  [EnumMember(Value = "basement")]
  Basement,
}

public enum ItemClassification
{
  [EnumMember(Value = "progression")]
  Progression,
  [EnumMember(Value = "filler")]
  Filler,
  [EnumMember(Value = "trap")]
  Trap,
}

public enum FillerType
{
  [EnumMember(Value = "junk")]
  Junk,
  [EnumMember(Value = "powerups")]
  Powerups,
  [EnumMember(Value = "traps")]
  Traps,
}
