namespace RhythmDoctor.Archipelago.Modifiers;

internal abstract class ModifierManagerBase : IDisposable
{
  private List<IModifier> _chosenModifiers = [];
  private List<IModifier> _previewModifiers = [];
  private List<IModifier> _activeModifiers = [];

  /// <summary>
  /// Attempts to add a modifier to <see cref="_chosenModifiers"/>.
  /// </summary>
  /// <param name="modifierUid">UID of the modifier to add.</param>
  /// <returns>True if the modifier was added.</returns>
  public bool TryAddModifier(string modifierUid)
  {
    // Check if modifier UID is valid.
    if (ModifierRegistry.TryGetModifier(modifierUid, out IModifier modifier))
    {
      Plugin.Logger.LogWarning(
        $"[{nameof(ModifierManagerBase)}] Attempted to add non-existant modifier {modifierUid}, ignoring"
      );
      return false;
    }

    return TryAddModifier(modifier);
  }

  /// <summary>
  /// Attempts to add a modifier to <see cref="_chosenModifiers"/>.
  /// </summary>
  /// <param name="modifier">Modifier to add.</param>
  /// <returns>True if the modifier was added.</returns>
  public bool TryAddModifier(IModifier modifier)
  {
    // Check if modifier is duplicated.
    if (_chosenModifiers.Contains(modifier))
    {
      Plugin.Logger.LogWarning(
        $"[{nameof(ModifierManagerBase)}] Attempted to add already existing modifier {modifier.Uid}, ignoring"
      );
      return false;
    }

    Plugin.Logger.LogInfo($"[{nameof(ModifierManagerBase)}] Adding modifier {modifier.Uid}");
    _chosenModifiers.Add(modifier);
    return true;
  }
  
  public void ClearAllChosenModifiers()
  {
    _chosenModifiers.Clear();
  }
  
  public void ClearAllPreviewModifiers()
  {
    foreach (IModifier modifier in _previewModifiers)
    {
      modifier.PreviewEnd();
    }
    _previewModifiers.Clear();
  }

  public void ClearAllActiveModifiers()
  {
    foreach (IModifier modifier in _activeModifiers)
    {
      modifier.PreviewEnd();
    }
    _activeModifiers.Clear();
  }

  public void ClearAllModifiers()
  {
    ClearAllChosenModifiers();
    ClearAllPreviewModifiers();
    ClearAllActiveModifiers();
  }

  /// <summary>
  /// Attempts to apply chosen modifiers to preview.
  /// </summary>
  /// <param name="level">Level to check against for compatibility.</param>
  /// <returns>True if any modifiers were applied.</returns>
  internal protected bool TryApplyChosenModifiersForLevel(Level level)
  {
    // do not iterate multiple times
    bool any = false;
    foreach (IModifier modifier in GetModifiersForLevel(level))
    {
      any = true;
      _chosenModifiers.Remove(modifier);
      _previewModifiers.Add(modifier);
      modifier.Preview(GetModifierStrength(modifier.Uid));
    }
    return any;
  }

  internal protected bool PromotePreviewModifiers()
  {
    _activeModifiers = new List<IModifier>(_previewModifiers);
    _previewModifiers.Clear();
    
    // do not iterate multiple times
    bool any = false;
    foreach (IModifier modifier in _activeModifiers)
    {
      any = true;
      modifier.Active(GetModifierStrength(modifier.Uid));
    }
    return any;
  }

  protected abstract float GetModifierStrength(string modifierUid);
  
  private protected IEnumerable<IModifier> GetModifiersForLevel(Level level)
  {
    List<IModifier> modifiers = [];
    
    foreach (IModifier modifierToAdd in _chosenModifiers)
    {
      if (ModifierRegistry.Compatible(modifierToAdd, level, modifiers))
      {
        modifiers.Add(modifierToAdd);
      }
    }

    return modifiers;
  }

  internal IEnumerable<string> GetPreviewTrapNames()
  {
    foreach (IModifier modifier in _previewModifiers)
    {
      yield return RDString.Get(modifier.LocalizationKey);
    }
  }

  internal bool IsTrapActive(string modifierUid)
    => _activeModifiers.Any(modifier => modifier.Uid == modifierUid);
  
  public void Dispose()
  {
    Plugin.Logger.LogInfo($"[{nameof(ModifierManagerBase)}] Disposing");
    ClearAllModifiers();
  }
}
