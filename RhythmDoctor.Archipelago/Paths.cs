namespace RhythmDoctor.Archipelago;

internal static class Paths
{
  // ReSharper disable NullableWarningSuppressionIsUsed
  internal static string Assembly = null!;
  internal static string Assets = null!;
  internal static string Localization = null!;
  internal static string WardIcons = null!;

  // ReSharper enable NullableWarningSuppressionIsUsed

  internal static void PopulatePaths()
  {
    Assembly = Path.GetDirectoryName(Plugin.Instance.Info.Location)!;
    Assets = Path.Combine(Assembly, "Assets");
    Localization = Path.Combine(Assets, "Localization");
    WardIcons = Path.Combine(Assets, "WardIcons");
  }
}
