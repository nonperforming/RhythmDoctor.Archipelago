namespace RhythmDoctor.Archipelago.World.Enums;

public enum ItemClassification
{
  [EnumMember(Value = "progression")]
  Progression,

  [EnumMember(Value = "useful-progression")]
  UsefulProgression,

  [EnumMember(Value = "filler")]
  Filler,

  [EnumMember(Value = "trap")]
  Trap,
}
