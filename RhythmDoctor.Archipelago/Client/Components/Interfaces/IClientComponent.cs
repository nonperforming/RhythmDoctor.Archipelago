namespace RhythmDoctor.Archipelago.Client.Components.Interfaces;

internal interface IClientComponent
{
  internal IEnumerable<Type> AssistPatches => [];

  internal Task Enable(StoryClient client, ArchipelagoSession session);
}
