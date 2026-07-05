namespace RhythmDoctor.Archipelago.Client.Components.Interfaces;

internal virtual class IClientComponent
{
  internal Task Enable(ArchipelagoSession session)
  {
    foreach (Type assistPatch in AssistPatches)
    {
      Harmony.CreateAndPatchAll(assistPatch, Plugin.PATCH_ID_POST_LOGIN);
    }
    return Task.CompletedTask;
  }

  private IEnumerable<Type> AssistPatches => [];
}
