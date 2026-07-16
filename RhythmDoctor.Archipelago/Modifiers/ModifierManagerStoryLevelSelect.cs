namespace RhythmDoctor.Archipelago.Modifiers;

/// <remarks>Depends on <see cref="ArchipelagoModifierManagerClientComponent"/>.</remarks>
internal class ModifierManagerStoryLevelSelect : ModifierManagerBase, IDisposable
{
  internal ModifierManagerStoryLevelSelect()
  {
    Events.Instance.LevelDeselected += OnInstanceOnLevelDeselected;
  }

  private void OnInstanceOnLevelDeselected(object _, EventArgs _1)
  {
    ClearAllPreviewModifiers();
  }

  protected override float GetModifierStrength(string modifierUid)
  {
    throw new NotImplementedException();
  }

  public new void Dispose()
  {
    base.Dispose();
    Plugin.Logger.LogInfo($"[{nameof(ModifierManagerStoryLevelSelect)}] Disposing");
    Events.Instance.LevelDeselected -= OnInstanceOnLevelDeselected;
  }
}
