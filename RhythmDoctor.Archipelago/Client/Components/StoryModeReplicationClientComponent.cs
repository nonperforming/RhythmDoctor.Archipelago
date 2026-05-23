namespace RhythmDoctor.Archipelago.Client.Components;

internal sealed class StoryModeReplicationClientComponent : IReplicationClientComponent, IClientComponent
{
  public IEnumerable<Type> AssistPatches
  {
    get
    {
      yield return typeof(StoryModeStateReplicationPatch);
    }
  }

  public async Task Enable()
  {
    
  }
}
