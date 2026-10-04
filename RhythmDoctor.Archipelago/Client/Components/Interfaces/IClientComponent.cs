namespace RhythmDoctor.Archipelago.Client.Components.Interfaces;

/// <summary>
/// Archipelago client component.
/// </summary>
/// <seealso cref="IClientComponent"/>
/// <seealso cref="StoryClient"/>
internal interface IClientComponent
{
  /// <summary>
  /// Patches applied before enabling this client component.
  /// </summary>
  internal IEnumerable<Type> AssistPatches => [];

  /// <summary>
  /// Enable this client component.
  /// </summary>
  /// <param name="client">Archipelago client that this client component is under.</param>
  /// <param name="session">Connected Archipelago session.</param>
  /// <returns>A task that represents the client component enabling.</returns>
  internal Task Enable(StoryClient client, ArchipelagoSession session);
}
