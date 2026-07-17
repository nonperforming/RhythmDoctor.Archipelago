namespace RhythmDoctor.Archipelago.Modifiers;

/// <remarks>Depends on <see cref="ArchipelagoModifierManagerClientComponent"/>.</remarks>
internal abstract class ModifierManagerStoryLevelSelect : ModifierManagerBase, IDisposable
{
  internal ModifierManagerStoryLevelSelect()
  {
    Events.Instance.LevelDeselected += OnInstanceOnLevelDeselected;
  }

  private void OnInstanceOnLevelDeselected(object _, EventArgs _1)
  {
    ClearAllPreviewModifiers();
  }

  public new void Dispose()
  {
    base.Dispose();
    Plugin.Logger.LogInfo($"[{nameof(ModifierManagerStoryLevelSelect)}] Disposing");
    Events.Instance.LevelDeselected -= OnInstanceOnLevelDeselected;
  }
}
