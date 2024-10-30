using System.Runtime.Serialization;

namespace RhythmDoctor.Archipelago.World;

public struct ItemsData
{
  [EnumMember(Value = "levels")]
  public Dictionary<Ward, List<Item>> Levels;
  [EnumMember(Value = "keys")]
  public List<Item> Keys;
  [EnumMember(Value = "filler")]
  public Dictionary<FillerType, List<Item>> Filler;

  public ItemsData()
  {
    string yaml = DataFileHelper.GetDataFile(DataFileType.Items);
    IDeserializer deserializer = new DeserializerBuilder()
      .WithNamingConvention(HyphenatedNamingConvention.Instance)
      .Build();
    //Dictionary<string, object> deserialized = (deserializer.Deserialize(yaml) as Dictionary<string, object>)!;

    //this.Levels = (deserialized["levels"] as Dictionary<Ward, List<Item>>)!;
    //this.Keys =
  }
}

public struct Item
{
  [EnumMember(Value = "name")]
  public string Name;
  [EnumMember(Value = "id")]
  public ulong ID;
  [EnumMember(Value = "classification")]
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

public enum ItemType
{
  [EnumMember(Value = "levels")]
  Levels,
  [EnumMember(Value = "keys")]
  Keys,
  [EnumMember(Value = "filler")]
  Filler,
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
