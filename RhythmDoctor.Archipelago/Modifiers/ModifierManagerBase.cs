namespace RhythmDoctor.Archipelago.Modifiers;

internal abstract class ModifierManagerBase : IDisposable
{
  private List<IModifier> _previewModifiers = new();
  private List<IModifier> _activeModifiers = new();

  public bool TryAddModifier(string modifierUid, out IEnumerable<IModifier> modifiers)
  {
    throw new NotImplementedException();
  }

  public bool TryAddModifier(IModifier modifier, out IEnumerable<IModifier> modifiers)
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
    Plugin.Logger.LogInfo("Disposing of TrapManager");

    Events.Instance.LevelDeselected -= OnLevelDeselected;

    foreach ((int _, ITrap trap) in _activeTraps)
    {
      trap.ActiveEnd();
    }

    foreach ((int _, ITrap trap) in _previewTraps)
    {
      trap.PreviewLevelEnd();
    }
  }
}
