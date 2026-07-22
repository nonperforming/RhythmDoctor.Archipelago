namespace RhythmDoctor.Archipelago.Modifiers;

internal static class ModifierRegistry
{
  private static Dictionary<string, IModifier> _uidToModifier = new();

  internal static void Register(IModifier modifier)
  {
    if (_uidToModifier.ContainsKey(modifier.Uid))
    {
      // TODO: consider using custom exception
      throw new Exception($"Trap '{modifier.Uid}' already registered");
    }

    Plugin.Logger.LogInfo($"[{nameof(ModifierRegistry)}] Registering trap {modifier.Uid}");
    _uidToModifier.Add(modifier.Uid, modifier);
  }

  internal static void Register(params IModifier[] modifiers) => modifiers.Do(Register);

  internal static bool TryGetModifier(string uid, out IModifier modifier)
  {
    return _uidToModifier.TryGetValue(uid, out modifier);
  }

  // /// <summary>
  // ///
  // /// </summary>
  // /// <param name="toAdd"></param>
  // /// <param name="other"></param>
  // /// <param name="level"></param>
  // /// <returns></returns>
  // /// <remarks>
  // /// Prefer using <see cref="Compatible(IModifier, IModifier, Level)"/> whenever possible.
  // /// </remarks>
  // internal static bool Compatible(IModifier toAdd, IModifier other, Level level = Level.None)
  // {
  // }

  internal static IEnumerable<string> GetAllRegisteredTrapsUid()
  {
    return _uidToModifier.Keys;
  }

  internal static bool Compatible(IModifier toAdd, Level level = Level.None, params IEnumerable<IModifier> others)
  {
    //if (!others.All((IModifier other) => Compatible(toAdd, other)))
    //{
    //  return false;
    //}

    if (toAdd.Compatibility.blacklistedLevels.Contains(level))
      return false;

    // Group 'others' into strength and mod
    Dictionary<IModifier, int> strength = new();
    foreach (IModifier modifier in others)
    {
      if (strength.ContainsKey(modifier))
      {
        strength[modifier] += 1;
      }
      else
      {
        strength[modifier] = 1;
      }
    }

    foreach (IModifier other in others)
    {
      // Checking strength scales cheaper than the others so we do it first
      //todo min
      // todo max
      //if (toAdd.Compatibility.maxStrength <= strength[other])
      //{
      //
      //}

      if (
        toAdd.Compatibility.blacklistedCapabilities.Any(
          (ModifierCapability otherCompat) => toAdd.Compatibility.blacklistedCapabilities.Contains(otherCompat)
        )
      )
        return false;
      if (
        toAdd.Compatibility.blacklistedModifierUids is not null
        && toAdd.Compatibility.blacklistedModifierUids.Contains(other.Uid)
      )
        return false;
    }

    // All checks passed
    return true;
  }
}
