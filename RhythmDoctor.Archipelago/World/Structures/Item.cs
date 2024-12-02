namespace RhythmDoctor.Archipelago.World.Structures;

public struct Item
{
  [YamlMember(Alias = "name")]
  public string Name;

  [YamlMember(Alias = "id")]
  public uint ID;

  [YamlMember(Alias = "classification")]
  public ItemClassification Classification;
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
