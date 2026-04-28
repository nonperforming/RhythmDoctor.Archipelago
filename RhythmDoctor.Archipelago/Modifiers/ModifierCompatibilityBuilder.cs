namespace RhythmDoctor.Archipelago.Modifiers;

// TODO: Source generator!!!
internal class ModifierCompatibilityBuilder
{
  private List<Level> _blacklistedLevels = [];
  private List<ModifierCapability> _blacklistedCapabilities = [];
  private double _minStrength = 1;
  private double _maxStrength = 1;
  private List<string> _blacklistedModifierUids = [];

  internal ModifierCompatibilityBuilder AddBlacklistedLevels(params IEnumerable<Level> levels)
  {
    _blacklistedLevels.AddRange(levels);
    return this;
  }

  internal ModifierCompatibilityBuilder AddBlacklistedCapabilities(params IEnumerable<ModifierCapability> capabilities)
  {
    _blacklistedCapabilities.AddRange(capabilities);
    return this;
  }

  internal ModifierCompatibilityBuilder SetMinimumStrength(double strength)
  {
    _minStrength = strength;
    return this;
  }
  
  private ModifierCompatibilityBuilder AddBlacklistedModifiers(params IEnumerable<string> uids)
  {
    _blacklistedModifierUids.AddRange(uids);
    return this;
  } 

  internal ModifierCompatibilityBuilder AddBlacklistedModifiers(params IEnumerable<IModifier> mods)
    => AddBlacklistedModifiers(mods.Select(mod => mod.Uid));
  
  internal ModifierCompatibilityBuilder SetMaximumStrength(double strength)
  {
    _maxStrength = strength;
    return this;
  }
  
  internal ModifierCompatibility Build()
  {
    return new ModifierCompatibility()
    {
      blacklistedLevels = _blacklistedLevels.Count == 0 ? null : _blacklistedLevels.ToArray(),
      blacklistedCapabilities = _blacklistedCapabilities.Count == 0 ? null : _blacklistedCapabilities.ToArray(),
      minStrength = _minStrength,
      maxStrength = _maxStrength,
      blacklistedModifierUids = _blacklistedModifierUids.Count == 0 ? null : _blacklistedModifierUids.ToArray(),
    };
  }
  
  internal static ModifierCompatibilityBuilder GetDefaultBuilderForMod(IModifier modifier)
  {
    return new ModifierCompatibilityBuilder()
      .AddBlacklistedCapabilities(modifier.Capabilities);
  }
  
  internal static ModifierCompatibility GetDefaultCompatibilityForMod(IModifier modifier)
  {
    return ModifierCompatibilityBuilder.GetDefaultBuilderForMod(modifier)
      .Build();
  }
}
