namespace RhythmDoctor.Archipelago.Helpers;

internal static class DataHelper
{
  private static readonly IDeserializer Deserializer = new DeserializerBuilder()
    .WithNamingConvention(HyphenatedNamingConvention.Instance)
    .Build();

  internal static string GetDataFile(DataFileType fileType)
  {
    string fileName = fileType switch
    {
      DataFileType.Items => "items.yml",
      DataFileType.Locations => "locations.yml",
      DataFileType.World => "world.yml",
      _ => throw new ArgumentOutOfRangeException(nameof(fileType), fileType, "File is not supported"),
    };

    string path = Path.Combine(Paths.Data, fileName);
    return File.ReadAllText(path);
  }

  internal static ItemsData GetItemsData()
  {
    string itemsData = GetDataFile(DataFileType.Items);
    return Deserializer.Deserialize<ItemsData>(itemsData);
  }

  internal static LocationsData GetLocationsData()
  {
    string locationsData = GetDataFile(DataFileType.Locations);
    return Deserializer.Deserialize<LocationsData>(locationsData);
  }
}

public enum DataFileType
{
  Items,
  Locations,
  World,
}
