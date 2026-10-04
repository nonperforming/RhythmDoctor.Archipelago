namespace RhythmDoctor.Archipelago.Modifiers;

internal abstract class ModifierManagerStoryLevelSelect : ModifierManagerBase, IDisposable
{
  internal ModifierManagerStoryLevelSelect()
  {
    Events.Instance.LevelDeselected += OnLevelDeselected;
  }

  private void OnLevelDeselected(object _, EventArgs _1)
  {
    Plugin.Logger.LogDebug($"[{nameof(ModifierManagerStoryLevelSelect)}] Level deselected");
    ClearAllPreviewModifiers();
  }

  public new void Dispose()
  {
    base.Dispose();
    Plugin.Logger.LogInfo($"[{nameof(ModifierManagerStoryLevelSelect)}] Disposing");
    Events.Instance.LevelDeselected -= OnLevelDeselected;
  }
}
