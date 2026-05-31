namespace RhythmDoctor.Archipelago.Client.Components.Interfaces;

internal interface IClientComponent
{
  internal Task Enable(ArchipelagoSession session);

  internal IEnumerable<Type> AssistPatches => [];
}
