namespace RhythmDoctor.Archipelago.Client;

/// <summary>
/// Login information for a player's slot.
/// </summary>
/// <param name="mode">The type of APWorld.</param>
/// <param name="uri">URI to the Archipelago server.</param>
/// <param name="slotName">Archipelago slot name to connect under.</param>
/// <param name="password">Password for the Archipelago server.</param>
internal struct LoginInformation(Mode mode, string uri, string slotName, string? password = null)
{
  /// <summary>
  /// The type of APWorld.
  /// </summary>
  internal readonly Mode Mode = mode;

  /// <summary>
  /// URI to the Archipelago server.
  /// </summary>
  internal readonly string Uri = uri;

  /// <summary>
  /// Archipelago slot name to connect under.
  /// </summary>
  internal readonly string SlotName = slotName;

  /// <summary>
  /// Password for the Archipelago server.
  /// </summary>
  internal readonly string? Password = password;
}
