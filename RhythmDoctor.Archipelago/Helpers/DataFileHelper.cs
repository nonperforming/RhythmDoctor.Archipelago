using System.Reflection;

namespace RhythmDoctor.Archipelago.Helpers;

internal static class DataFileHelper
{
  internal static string GetDataFile(DataFileType fileType)
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
}

internal enum DataFileType
{
  Items,
  Locations,
  Options,
  World,
}
