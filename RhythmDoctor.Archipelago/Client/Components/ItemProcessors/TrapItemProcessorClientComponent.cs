namespace RhythmDoctor.Archipelago.Client.Components.ItemProcessors;

internal class TrapItemProcessorClientComponent : ItemProcessorClientComponent
{
  private Dictionary<string, uint> _localTrapClearCache = new();
  private Dictionary<string, uint> _remoteTrapClearCache = new();

  public override async Task Enable(StoryClient client, ArchipelagoSession session)
  {
    await base.Enable(client, session);

    Plugin.Logger.LogInfo($"[{nameof(TrapItemProcessorClientComponent)}] Enabling...");

    // TODO: could be made lazy :ppp
    // Get remote trap cache
    foreach (string trapUid in ModifierRegistry.GetAllRegisteredTrapsUid())
    {
      // FIXME: this can fail sometimes, retry if necessary
      _localTrapClearCache[trapUid] = 0;
      _remoteTrapClearCache[trapUid] = (uint)_session.DataStorage[Scope.Slot, trapUid];
    }

    Plugin.Logger.LogInfo($"[{nameof(TrapItemProcessorClientComponent)}] Enabled");

    // Add sticky traps
    // TODO:
  }

  internal override bool HandleItemInitial(ItemInfo itemInfo)
  {
    if (!Bindings.ModifierItemIdToModifierUid.TryGetValue(itemInfo.ItemId, out string trapUid))
      return false; // Not a trap

    // Check remote cache if this has been cleared already
    uint local = _localTrapClearCache[trapUid];
    uint remote = _remoteTrapClearCache[trapUid];
    if ((local <= remote) && (remote != 0))
    {
      Plugin.Logger.LogDebug(
        $"[{nameof(TrapItemProcessorClientComponent)}] Trap {trapUid} already cleared, skipping (l: {local} <= r: {remote})"
      );
      _localTrapClearCache[trapUid]++;
      return true;
    }

    // Not cleared already, add to trap manager
    Plugin.Logger.LogInfo(
      $"[{nameof(TrapItemProcessorClientComponent)}] Trap {trapUid} not cleared previously, handling normally"
    );
    return HandleItem(itemInfo);
  }

  internal override bool HandleItem(ItemInfo itemInfo)
  {
    if (!Bindings.ModifierItemIdToModifierUid.TryGetValue(itemInfo.ItemId, out string trapUid))
      return false; // Not a trap

    Plugin.Logger.LogInfo($"[{nameof(TrapItemProcessorClientComponent)}] Handling trap {trapUid}");

    _client.ModifierManagerComponent!.AddModifierToQueue(trapUid);
    return true;
  }
}
