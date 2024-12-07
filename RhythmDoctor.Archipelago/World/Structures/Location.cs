namespace RhythmDoctor.Archipelago.World.Structures;

public struct Location
{
  [YamlMember(Alias = "name")]
  public string Name;

  [YamlMember(Alias = "id")]
  public long ID;

  [YamlMember(Alias = "classification")]
  public LocationClassification Classification;
}
