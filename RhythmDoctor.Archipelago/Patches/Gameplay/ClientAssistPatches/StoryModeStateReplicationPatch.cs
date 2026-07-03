namespace RhythmDoctor.Archipelago.Patches.Gameplay.ClientAssistPatches;


/// <summary>
/// Helper patch for <see cref="StoryReplicationClientComponent"/>
/// </summary>
[HarmonyPatch(typeof(StoryReplicationClientComponent))]
internal static class StoryModeStateReplicationPatch
{
  // FIXME: Make async and retry on failure!!

  /// <seealso cref="StoryReplicationClientComponent.ReplicatePaigeStays"/>
  [HarmonyPatch(nameof(Persistence.SetPaigeEnding))]
  [HarmonyPostfix]
  private static void ReplicatePaigeStaysPatch(bool paigeStays)
  {
    Plugin.Logger.LogInfo($"Replicating {Persistence.PaigeStaysKey} to {paigeStays}");
    Plugin.StoryClient.ReplicationComponent?.UpdateRemote(Persistence.PaigeStaysKey, paigeStays);
  }

  /// <seealso cref="StoryReplicationClientComponent.ReplicateIansDesktopUnlocked"/>
  [HarmonyPatch(nameof(Persistence.SetIanDesktopLogin))]
  [HarmonyPostfix]
  private static void ReplicateIansDesktopUnlockedPatch(bool ianDesktopLogin)
  {
    Plugin.Logger.LogInfo($"Replicating {Persistence.IanDesktopLoginKey} to {ianDesktopLogin}");
    Plugin.StoryClient.Session!.DataStorage[Scope.Slot, Persistence.IanDesktopLoginKey] = ianDesktopLogin;
  }
}
