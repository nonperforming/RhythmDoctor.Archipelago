namespace RhythmDoctor.Archipelago.Modifiers;

internal class ModifierManagerStoryLevelSelect : ModifierManagerBase, IDisposable
{
  internal ModifierManagerStoryLevelSelect()
  {
    Events.Instance.LevelDeselected += OnInstanceOnLevelDeselected;
  }

  private void OnInstanceOnLevelDeselected(object _, EventArgs _1)
  {
    ClearAllPreviewTraps();
  }

  public new void Dispose()
  {
    base.Dispose();
    Plugin.Logger.LogInfo($"[{nameof(ModifierManagerStoryLevelSelect)}] Disposing");
    Events.Instance.LevelDeselected -= OnInstanceOnLevelDeselected;
  }
}
