using System.Reflection;

namespace RhythmDoctor.Archipelago.Helpers;

internal static class DataFileHelper
{
  private static IDeserializer Deserializer = new DeserializerBuilder()
    .WithNamingConvention(HyphenatedNamingConvention.Instance)
    .Build();

  private static string GetDataFile(DataFileType fileType)
  {
    string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

    string fileName;
    switch (fileType)
    {
      case DataFileType.Items:
        fileName = "items.yml";
        break;
      case DataFileType.Locations:
        fileName = "locations.yml";
        break;
      case DataFileType.Options:
        fileName = "options.yml";
        break;
      case DataFileType.World:
        fileName = "world.yml";
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(fileType), fileType, "File type is not supported.");
    }

    string path = Path.Combine(assemblyFolder, "World", "data", fileName);
    return File.ReadAllText(path);
  }

  internal static ItemsData GetItemsData()
  {
    string itemsData = GetDataFile(DataFileType.Items);
    return Deserializer.Deserialize<ItemsData>(itemsData);
  }
}

internal enum DataFileType
{
  Items,
  Locations,
  Options,
  World,
}
