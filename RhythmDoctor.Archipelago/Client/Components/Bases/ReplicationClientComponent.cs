namespace RhythmDoctor.Archipelago.Client.Components.Interfaces;

internal abstract class ReplicationClientComponent : IClientComponent
{
  private ArchipelagoSession _session;
  
  public virtual Task Enable(ArchipelagoSession session)
  {
    _session = session;
    base.Enable();
    return Task.CompletedTask;
  }
  
  internal void UpdateRemote(string key, object value)
  {
    Plugin.Logger.LogInfo($"[{nameof(ReplicationClientComponent)}] Replicating {key} to {value}");
    _session.DataStorage[Scope.Slot, key] = (DataStorageElement)value;
  }
}
