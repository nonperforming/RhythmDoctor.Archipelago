namespace RhythmDoctor.Archipelago.Modifiers.Archipelago;

internal interface IArchipelagoModifier
{
  /// <summary>
  /// The scale used for this modifier.
  /// </summary>
  /// <returns><see cref="IScale"/> instance that should be used.</returns>
  /// <seealso cref="IScale.GetScale"/>
  [Pure]
  IScale Scale { get; }
}
