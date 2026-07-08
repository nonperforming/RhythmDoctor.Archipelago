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
        $"[{nameof(ModifierManagerBase)}] Attempted to add already existing modifier {modifierUid}, ignoring"
      );
      return false;
    }

    Plugin.Logger.LogInfo($"[{nameof(ModifierManagerBase)}] Adding modifier {modifierUid}");
    _chosenModifiers.Add(modifier);
    return true;
  }
  
  public void ClearAllChosenTraps()
  {
    _chosenModifiers.Clear();
  }
  
  public void ClearAllPreviewTraps()
  {
    foreach (IModifier modifier in _previewModifiers)
    {
      modifier.PreviewEnd();
    }
    _previewModifiers.Clear();
  }

  public void ClearAllActiveTraps()
  {
    foreach (IModifier modifier in _activeModifiers)
    {
      modifier.PreviewEnd();
    }
    _activeModifiers.Clear();
  }

  public void ClearAllTraps()
  {
    ClearAllChosenTraps();
    ClearAllPreviewTraps();
    ClearAllActiveTraps();
  }
  
  internal protected IEnumerable<IModifier> GetModifiersForLevel(Level level)
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

  public void Dispose()
  {
    Plugin.Logger.LogInfo($"[{nameof(ModifierManagerBase)}] Disposing");
    ClearAllTraps();
  }
}
