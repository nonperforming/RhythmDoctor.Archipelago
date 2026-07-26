namespace RhythmDoctor.Archipelago.Modifiers.Archipelago;

internal abstract class ScaleBase<T> : Lazy<T>, IScale
  where T : new()
{
  private static readonly Lazy<T> lazy = new(() => new T());
  public static T Instance => lazy.Value;

  /// <inheritdoc/>
  public abstract float GetScale(int num, out int consumed);
}
