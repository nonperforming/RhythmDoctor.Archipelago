namespace RhythmDoctor.Archipelago.Modifiers;

internal abstract class ModifierManagerBase : IDisposable
{
  private List<IModifier> _previewModifiers = new();
  private List<IModifier> _activeModifiers = new();

  /// <summary>
  /// T
  /// </summary>
  /// <param name="modifierUid"></param>
  /// <param name="modifiers"></param>
  /// <returns>True if any </returns>
  public bool TryAddModifier(string modifierUid, Level level, out IEnumerable<IModifier> modifiers)
  {
    foreach (IModifier previewModifier in _previewModifiers)
    {
      // Not a valid modifier.
      Plugin.Logger.LogWarning(
        $"[{nameof(ModifierManagerBase)}] Attempted to add non-existant modifier {modifierUid}, ignoring"
      );
      return;
    }

    if (chosenModifiers.Contains(modifier))
    {
      // Duplicated.
      Plugin.Logger.LogWarning(
        $"[{nameof(ModifierManagerBase)}] Attempted to add already existing modifier {modifierUid}, ignoring"
      );
      return;
    }

    Plugin.Logger.LogInfo($"[{nameof(ModifierManagerBase)}] Adding modifier {modifierUid}");
    chosenModifiers.Add(modifier);
  }

  public bool TryAddModifier(IModifier modifier, Level level, out IEnumerable<IModifier> modifiers)
  {
    throw new NotImplementedException();
  }

  public void ClearAllPreviewTraps()
  {
    throw new NotImplementedException();
  }

  public void ClearAllActiveTraps()
  {
    throw new NotImplementedException();
  }

  public void ClearAllTraps()
  {
    throw new NotImplementedException();
  }

  public void Dispose()
  {
    Plugin.Logger.LogInfo($"[{nameof(ModifierManagerBase)}] Disposing");
    throw new NotImplementedException();
  }
}
