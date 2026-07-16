namespace RhythmDoctor.Archipelago.Client.Components.ItemProcessors;

internal class TrapItemProcessorClientComponent : ItemProcessorClientComponent
{
  private Dictionary<string, uint> _localTrapClearCache = new();
  private Dictionary<string, uint> _remoteTrapClearCache = new();
  
  internal override async Task Enable(StoryClient client, ArchipelagoSession session)
  {
    await base.Enable(client, session);
    
    // TODO: could be made lazy :ppp
    // Get remote trap cache
    foreach (string trapUid in ModifierRegistry.GetAllRegisteredTrapsUid())
    {
      // FIXME: this can fail sometimes, retry if necessary
      _localTrapClearCache[trapUid] = 0;
      _remoteTrapClearCache[trapUid] = (uint)_session.DataStorage[Scope.Slot, trapUid];
    }
    
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
      Plugin.Logger.LogDebug($"[{nameof(TrapItemProcessorClientComponent)}] Trap {trapUid} already cleared, skipping (l: {local} <= r: {remote})");
      _localTrapClearCache[trapUid]++;
      return true;
    }

    // Not cleared already, add to trap manager
    return HandleItem(itemInfo);
  }

  internal override bool HandleItem(ItemInfo itemInfo)
  {
    if (!Bindings.ModifierItemIdToModifierUid.TryGetValue(itemInfo.ItemId, out string trapUid))
      return false; // Not a trap

    _client.ModifierManagerComponent.TryAddModifier(trapUid);
    return true;
  }
}
