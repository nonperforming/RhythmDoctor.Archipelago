namespace RhythmDoctor.Archipelago.Extensions;

internal static class RandomExtensions
{
  /// <summary>
  /// Shuffle an array using the Fisher-Yates algorithm.
  /// </summary>
  /// <remarks>From https://stackoverflow.com/a/110570.</remarks>
  /// <param name="rng">Random instance.</param>
  /// <param name="array">Array to randomize.</param>
  /// <typeparam name="T">Array type.</typeparam>
  public static void Shuffle<T>(this Random rng, T[] array)
  {
    int n = array.Length;
    while (n > 1)
    {
      int k = rng.Next(n--);
      (array[n], array[k]) = (array[k], array[n]);
    }
  }
}
