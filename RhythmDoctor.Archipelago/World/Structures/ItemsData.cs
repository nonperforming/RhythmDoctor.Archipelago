namespace RhythmDoctor.Archipelago.World.Structures;

public struct ItemsData
{
  [YamlMember(Alias = "levels")]
  public Dictionary<Ward, List<Item>> Levels;
  [YamlMember(Alias = "keys")]
  public List<Item> Keys;
  [YamlMember(Alias = "filler")]
  public Dictionary<FillerType, List<Item>> Filler;
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
