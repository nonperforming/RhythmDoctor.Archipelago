namespace RhythmDoctor.Archipelago.Modifiers;

internal class ModifierManagerStoryLevelSelect : ModifierManagerBase, IDisposable
{
  internal ModifierManagerStoryLevelSelect()
  {
    Events.Instance.LevelDeselected += (_, _) => ClearAllPreviewTraps();
  }
}
