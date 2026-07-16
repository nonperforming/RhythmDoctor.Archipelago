namespace RhythmDoctor.Archipelago.Client.Components;

internal class ArchipelagoModifierManagerClientComponent : ModifierManagerStoryLevelSelect, IClientComponent
{
  private List<string> _queueModifiers = new();

  public IEnumerable<Type> AssistPatches => [typeof(ModifierManagerPatch)];

  public Task Enable(ArchipelagoSession session) => Task.CompletedTask;

  internal void AddModifierToQueue(string modifierUid)
  {
    if (!ModifierRegistry.TryGetModifier(modifierUid, out IModifier modifier))
      throw new ArgumentException($"Unknown modifier {modifierUid}");

    _queueModifiers.Add(modifierUid);
  }
}
