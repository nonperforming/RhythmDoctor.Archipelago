namespace RhythmDoctor.Archipelago.Client.Components.Interfaces;

/// <summary>
/// Archipelago client component base.
/// Sets <see cref="_client"/> and <see cref="_session"/> fields
/// and applies <see cref="AssistPatches"/> patches when enabling
/// the component.
/// </summary>
/// <seealso cref="IClientComponent"/>
/// <seealso cref="StoryClient"/>
internal abstract class ClientComponentBase : IClientComponent
{
  /// <summary>
  /// The Archipelago client that this client component is under.
  /// </summary>
  internal StoryClient _client = null!;

  /// <summary>
  /// The connected Archipelago session.
  /// </summary>
  internal ArchipelagoSession _session = null!;

  /// <inheritdoc cref="IClientComponent.AssistPatches" />
  private IEnumerable<Type> AssistPatches => [];

  /// <inheritdoc/>
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
