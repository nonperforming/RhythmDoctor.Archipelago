namespace RhythmDoctor.Archipelago.Client.Components.Interfaces;

internal abstract class ReplicationClientComponent : ClientComponentBase
{
  internal void UpdateRemote(string key, object value)
  {
    Plugin.Logger.LogInfo($"[{nameof(ReplicationClientComponent)}] Replicating {key} to {value}");
    _session.DataStorage[Scope.Slot, key] = (DataStorageElement)value;
  }
}
