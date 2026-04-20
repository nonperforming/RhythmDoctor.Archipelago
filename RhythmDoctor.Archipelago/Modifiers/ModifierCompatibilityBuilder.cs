namespace RhythmDoctor.Archipelago.Modifiers;

internal class ModifierCompatibilityBuilder
{
  internal static readonly ModifierCompatibilityBuilder Default = new ModifierCompatibilityBuilder();

  internal ModifierCompatibility Build()
  {
    return new ModifierCompatibility();
  }
}
