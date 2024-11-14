namespace RhythmDoctor.Archipelago;

internal static class Paths
{
  // ReSharper disable once NullableWarningSuppressionIsUsed
  internal static readonly string Assembly = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
  internal static readonly string Data = Path.Combine(Assembly, "World", "data");
  internal static readonly string Assets = Path.Combine(Assembly, "World", "assets");
}
