namespace RhythmDoctor.Archipelago.Modifiers.Archipelago;

internal interface IScale
{
  /// <summary>
  /// Gets the scale for this modifier.
  /// </summary>
  /// <param name="num">The highest number of modifier items that can be consumed.</param>
  /// <param name="consumed">The amount of modifier items that should be consumed.</param>
  /// <returns>The scale that should be used for the trap.</returns>
  internal float GetScale(int num, out int consumed);
}
