namespace RhythmDoctor.Archipelago.Client.Components.ItemProcessors;

using Newtonsoft.Json.Linq;

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
      // FIXME: this could rarely fail, retry if necessary
      _localTrapClearCache[trapUid] = 0;
      uint remote;
      JToken remoteJToken = await _session.DataStorage[Scope.Slot, trapUid].GetAsync();

      if (!remoteJToken.HasValues) // == null doesn't work
        remote = 0;
      else
        remote = remoteJToken.ToObject<uint>();

      _remoteTrapClearCache[trapUid] = remote;
    }

    Plugin.Logger.LogInfo($"[{nameof(TrapItemProcessorClientComponent)}] Enabled");
  }

  internal override bool HandleItemInitial(ItemInfo itemInfo)
  {
    if (!Bindings.ModifierItemIdToModifierUid.TryGetValue(itemInfo.ItemId, out string trapUid))
      return false; // Not a trap

    // Do not add already cleared traps
    uint local = _localTrapClearCache[trapUid]++; // local skipped traps
    uint remote = _remoteTrapClearCache[trapUid]; // max cleared traps in DataStorage

    if (local > remote)
    {
      Plugin.Logger.LogDebug(
        $"[{nameof(TrapItemProcessorClientComponent)}] Trap {trapUid} already cleared, skipping (l: {local} < r: {remote})"
      );
      return true;
    }

    // Not cleared already, add to modifier manager
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
