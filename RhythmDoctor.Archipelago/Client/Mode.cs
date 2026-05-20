namespace RhythmDoctor.Archipelago.Client;

/// <summary>
/// Archipelago client mode.
/// </summary>
internal enum Mode
{
  /// <summary>
  /// The primary mode of the client, randomizes levels and wards behind items.
  /// Clearing standard levels with B (Clear)/A (Complete+)/S (Perfect) ranks,
  /// clearing Rhythm Weightlifter stages sends locations.
  /// </summary>
  Main,
}
