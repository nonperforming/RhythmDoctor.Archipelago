namespace RhythmDoctor.Archipelago.Client.Components.Interfaces;

internal abstract class ClientComponentBase : IClientComponent
{
  internal StoryClient _client = null!;
  internal ArchipelagoSession _session = null!;

  private IEnumerable<Type> AssistPatches => [];

  public virtual Task Enable(StoryClient client, ArchipelagoSession session)
  {
    _client = client;
    _session = session;
    foreach (Type assistPatch in AssistPatches)
    {
      Harmony.CreateAndPatchAll(assistPatch, Plugin.PATCH_ID_POST_LOGIN);
    }
    return Task.CompletedTask;
  }
}
