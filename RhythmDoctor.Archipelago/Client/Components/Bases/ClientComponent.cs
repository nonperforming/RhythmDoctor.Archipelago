namespace RhythmDoctor.Archipelago.Client.Components.Interfaces;

internal abstract class ClientComponent
{
  internal StoryClient _client;
  internal ArchipelagoSession _session;

  internal virtual Task Enable(StoryClient client, ArchipelagoSession session)
  {
    _client = client;
    _session = session;
    foreach (Type assistPatch in AssistPatches)
    {
      Harmony.CreateAndPatchAll(assistPatch, Plugin.PATCH_ID_POST_LOGIN);
    }
    return Task.CompletedTask;
  }

  private IEnumerable<Type> AssistPatches => [];
}
