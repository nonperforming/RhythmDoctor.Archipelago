namespace RhythmDoctor.Archipelago;

internal static class Paths
{
  // TODO: Look into using Plugin.Info.Location
  //       Having issues with a clean implementation, as PluginInfo is non-static.

  // ReSharper disable once NullableWarningSuppressionIsUsed
  internal static readonly string Assembly = Path.GetDirectoryName(
    System.Reflection.Assembly.GetExecutingAssembly().Location
  )!;
  internal static readonly string Assets = Path.Combine(Assembly, "Assets");
  internal static readonly string Localization = Path.Combine(Assets, "Localization");
  internal static readonly string WardIcons = Path.Combine(Assets, "WardIcons");
}
