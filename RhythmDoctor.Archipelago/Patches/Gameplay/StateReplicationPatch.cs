namespace RhythmDoctor.Archipelago.Patches.Gameplay;

/// <seealso cref="Client"/>
[HarmonyPatch(typeof(Persistence))]
internal static class StateReplicationPatch
{
  /// <seealso cref="Client.ReplicatePaigeStays"/>
  [HarmonyPatch(nameof(Persistence.SetPaigeEnding))]
  [HarmonyPostfix]
  private static void ReplicatePaigeStaysPatch(bool paigeStays)
  {
    Plugin.Client.Session!.DataStorage[Scope.Slot, Persistence.PaigeStaysKey] = paigeStays;
  }
}
