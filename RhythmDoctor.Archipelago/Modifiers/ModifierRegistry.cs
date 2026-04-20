namespace RhythmDoctor.Archipelago.Modifiers;

internal static class ModifierRegistry
{
  private static List<IModifier> _registeredMods = new();

  internal static void Register(IModifier modifier)
  {
    
  }
  
  internal static void Register(params IModifier[] modifiers)
    => modifiers.Do(mod => _registeredMods.Add(mod));
  
  
}
