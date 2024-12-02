namespace RhythmDoctor.Archipelago.World;

public struct LocationsData
{
  public LocationsData()
  {
    Plugin.Logger?.LogInfo("Creating LocationsData");
    this = DataHelper.GetLocationsData();
  }

  public Dictionary<Area, Dictionary<LevelStage, List<Item>>> Levels;
}
