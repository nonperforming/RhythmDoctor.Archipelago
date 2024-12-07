namespace RhythmDoctor.Archipelago.World.Structures;

public struct Item
{
  [YamlMember(Alias = "name")]
  public string Name;

  [YamlMember(Alias = "id")]
  public ulong ID;

  [YamlMember(Alias = "classification")]
  public ItemClassification Classification;
}
