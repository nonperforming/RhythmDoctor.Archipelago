namespace RhythmDoctor.Archipelago.Modifiers.Archipelago;

// TODO: This should be inherited.........
internal static class Scales
{
  internal static double BinaryScale(int num, out int consumed)
  {
    if (num >= 1)
    {
      consumed = 1;
      return 1;
    }

    consumed = 0;
    return 0;
  }
}
