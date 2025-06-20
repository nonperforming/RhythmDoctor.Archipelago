namespace RhythmDoctor.Archipelago.Patches;

[HarmonyPatch(typeof(scnBase))]
static class UnapplyPatchesPatch
{
  [HarmonyPatch(nameof(scnBase.GoToMainMenu))]
  [HarmonyPrefix]
  internal static void TearDownClientPlugin()
  {
    // TODO: Correct way to tear down Archipelago client?
    if (Plugin.Client.session != null)
      // For some reason session does not want to accept '?'
      // NOTE: .Disconnect is documented at https://github.com/ArchipelagoMW/Archipelago.MultiClient.Net/blob/main/Archipelago.MultiClient.Net/Helpers/ArchipelagoSocketHelper_websocket-sharp.cs/#L136
      //   but .Socket only exposes DisconnectAsync?
      Task.Run(Plugin.Client.session.Socket.DisconnectAsync).Wait();
    Plugin.Client.session = null;
    Plugin.Client.deathLinkService = null;
    Plugin.Client.trapManager.ClearAllTraps(true);
    Plugin.UnapplyGameplayPatches();
  }
}
