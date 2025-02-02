namespace RhythmDoctor.Archipelago.World.Enums;

public enum LocationClassification
{
  [EnumMember(Value = "default")]
  Default,

  [EnumMember(Value = "priority")]
  Priority,

  [EnumMember(Value = "excluded")]
  Excluded,
}
