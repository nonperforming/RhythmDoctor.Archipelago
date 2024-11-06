namespace RhythmDoctor.Archipelago.World;

public struct LocationsData
{
  [YamlMember(Alias = "act-1")]
  public Dictionary<LevelStage, List<Item>> Act1Levels;

  [YamlMember(Alias = "act-2")]
  public Dictionary<LevelStage, List<Item>> Act2Levels;

  [YamlMember(Alias = "act-3")]
  public Dictionary<LevelStage, List<Item>> Act3Levels;

  [YamlMember(Alias = "act-4")]
  public Dictionary<LevelStage, List<Item>> Act4Levels;

  [YamlMember(Alias = "act-5")]
  public Dictionary<LevelStage, List<Item>> Act5Levels;

  [YamlMember(Alias = "basement")]
  public Dictionary<LevelStage, List<Item>> BasementLevels;

  [YamlMember(Alias = "art-room")]
  public Dictionary<LevelStage, List<Item>> ArtRoomLevels;
}
