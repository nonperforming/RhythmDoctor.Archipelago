namespace RhythmDoctor.Archipelago;

internal static class Paths
{
  internal static readonly string Assembly = Path.GetDirectoryName(Plugin.Instance.Info.Location)!;
  internal static readonly string Assets = Path.Combine(Assembly, "Assets");
  internal static readonly string Localization = Path.Combine(Assets, "Localization");
  internal static readonly string WardIcons = Path.Combine(Assets, "WardIcons");
}
