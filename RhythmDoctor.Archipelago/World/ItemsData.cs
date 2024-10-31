namespace RhythmDoctor.Archipelago.World;

public struct ItemsData
{
  [YamlMember(Alias = "levels")]
  public Dictionary<Ward, List<Item>> Levels;
  [YamlMember(Alias = "keys")]
  public List<Item> Keys;
  [YamlMember(Alias = "filler")]
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
  [YamlMember(Alias = "name")]
  public string Name;
  [YamlMember(Alias = "id")]
  public ulong ID;
  [YamlMember(Alias = "classification")]
  public ItemClassification Classification;
}

public enum Ward
{
  [YamlMember(Alias = "main-ward")]
  MainWard,
  [YamlMember(Alias = "svt-ward")]
  SVTWard,
  [YamlMember(Alias = "train")]
  Train,
  [YamlMember(Alias = "physiotherapy-ward")]
  PhysiotherapyWard,
  [YamlMember(Alias = "basement")]
  Basement,
}

public enum ItemType
{
  [YamlMember(Alias = "levels")]
  Levels,
  [YamlMember(Alias = "keys")]
  Keys,
  [YamlMember(Alias = "filler")]
  Filler,
}

public enum ItemClassification
{
  [YamlMember(Alias = "progression")]
  Progression,
  [YamlMember(Alias = "filler")]
  Filler,
  [YamlMember(Alias = "trap")]
  Trap,
}

public enum FillerType
{
  [YamlMember(Alias = "junk")]
  Junk,
  [YamlMember(Alias = "powerups")]
  Powerups,
  [YamlMember(Alias = "traps")]
  Traps,
}
