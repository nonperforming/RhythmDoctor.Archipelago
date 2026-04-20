namespace RhythmDoctor.Archipelago.Modifiers;

//traps should be compatible with most and only incompatible with few
// thus blacklist over whitelist
// everything should be implemented in this
// so no Func<bool>
// and explicit
// prefer blocking capabilities over mods

internal record struct ModifierCompatibility(Level[] blacklistedLevels, ModifierCapability[] blacklistedCapabilities, int maxCount, string[]? blacklistedModifiers = null);