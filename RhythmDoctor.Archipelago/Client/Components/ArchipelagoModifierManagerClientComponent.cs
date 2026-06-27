namespace RhythmDoctor.Archipelago.Client.Components;

internal class ArchipelagoModifierManagerClientComponent : ModifierManagerStoryLevelSelect, IClientComponent
{
  public Task Enable(ArchipelagoSession session) => Task.CompletedTask;

  internal void AddModifierToQueue(string modifierUid)
  {
    if (!ModifierRegistry.TryGetModifier(modifierUid, out IModifier modifier))
      throw new ArgumentException($"Unknown modifier {modifierUid}");

    _queueModifiers.Add(modifier);
  }
}
