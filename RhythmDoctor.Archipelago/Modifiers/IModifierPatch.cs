namespace RhythmDoctor.Archipelago.Modifiers;

internal abstract class IModifierPatch<T> : IModifier
{
  internal Harmony _harmony = new($"{Plugin.PATCH_ID_TRAP}.{nameof(T)}");
  
  public abstract string Name { get; }
  public abstract ModifierCompatibility Compatibility { get; }
  public abstract ModifierCapability Capability { get; }
  public abstract void Initialize();
  public abstract void Preview(int strength);
  public abstract void PreviewEnd();
  public abstract void Active(int strength);
  public abstract void ActiveEnd();
}
