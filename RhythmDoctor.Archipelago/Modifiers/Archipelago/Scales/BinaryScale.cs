namespace RhythmDoctor.Archipelago.Modifiers.Archipelago.Scales;

internal class BinaryScale : ScaleBase<BinaryScale>
{
  public override float GetScale(int num, out int consumed)
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
