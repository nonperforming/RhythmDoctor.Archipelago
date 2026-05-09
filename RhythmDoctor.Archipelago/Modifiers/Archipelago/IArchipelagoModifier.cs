namespace RhythmDoctor.Archipelago.Modifiers.Archipelago;

internal interface IArchipelagoModifier
{
  /// <summary>
  /// Gets how the trap's strength should scale
  /// based on how many item traps we have.
  /// </summary>
  /// <param name="num">Number of items we have for this trap. Must be at least 1.</param>
  /// <param name="consumed">Number of items that should be consumed for the strength given. Must be at least 1.</param>
  float GetScale(int num, out int consumed);
}
