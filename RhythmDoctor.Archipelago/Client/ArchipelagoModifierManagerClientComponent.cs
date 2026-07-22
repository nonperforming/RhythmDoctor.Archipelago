namespace RhythmDoctor.Archipelago.Client;

internal sealed class ArchipelagoModifierManagerClientComponent : ModifierManagerBase, IClientComponent, IDisposable
{
  internal IEnumerable<Type> AssistPatches = [typeof(ArchipelagoModifierManagerPatch)];

  private readonly List<string> _modifierQueue = [];
  private readonly List<(int index, string Uid)> _modifierAndIndexPairs = [];

  public Task Enable(StoryClient client, ArchipelagoSession session)
  {
    return Task.CompletedTask;
  }

  internal void ReturnActiveTrapsToQueue()
  {
    // We iterate in reverse because _trapAndIndexPairs is guaranteed to be ordered from the
    //  lowest index to the highest index, so we don't have to manipulate the index this way.
    for (int i = _modifierAndIndexPairs.Count - 1; i >= 0; i--)
    {
      (int index, string uid) = _modifierAndIndexPairs[i];

      Plugin.Logger.LogDebug(
        $"[{nameof(ArchipelagoModifierManagerClientComponent)}] Returning trap {uid} to index {index}."
      );
      _modifierQueue.Insert(index, uid);
    }
    _modifierAndIndexPairs.Clear();
  }

  protected override float GetModifierStrength(IModifier modifier)
  {
    if (modifier is not IArchipelagoModifier archipelagoModifier)
      throw new ArgumentException($"Trap must be {nameof(IArchipelagoModifier)}.");

    // Find the maximum scale we can get for each trap...
    // At this point the trap's in _previewModifiers.

    List<int> matchIndexes = _modifierQueue
      .Select((otherUid, i) => otherUid == modifier.Uid ? i : -1)
      .Where(i => i != -1)
      .ToList();
    float scale = archipelagoModifier.GetScale(matchIndexes.Count, out int consumed);

    // Remove 'consumed' amount of traps at their respective index, and add them to _trapAndIndexPairs.
    for (int i = 0; i < consumed; i++)
    {
      int indexToRemove = matchIndexes[i];
      _modifierAndIndexPairs.Add((indexToRemove, modifier.Uid));
    }

    return scale;
  }

  public new void Dispose()
  {
    base.Dispose();
  }
}
