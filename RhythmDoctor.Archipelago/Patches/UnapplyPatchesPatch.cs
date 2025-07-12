namespace RhythmDoctor.Archipelago.Patches;

[HarmonyPatch(typeof(scnBase))]
static class UnapplyPatchesPatch
{
  // Access modifier is internal instead of private as the finalizer for Plugin can call it
  // (i.e. when running under ScriptEngine).
  [HarmonyPatch(nameof(scnBase.GoToMainMenu))]
  [HarmonyPrefix]
  internal static void TearDownClientPlugin()
  {
    // TODO: Correct way to tear down Archipelago client?
    if (Plugin.Client.session != null)
      // For some reason session does not want to accept '?'
      // NOTE: Disconnect in a non-async manner is under NET35
      //       This should probably be made async.
      Task.Run(Plugin.Client.session.Socket.DisconnectAsync).Wait();
    //Plugin.Client.session = null;
    //Plugin.Client.deathLinkService = null;
    //Plugin.Client.trapManager.ClearAllTraps(true);
    Plugin.Client = null!; // TODO: Setting the Client to null should automatically clean up itself and children?
    Plugin.UnapplyGameplayPatches();
  }
}
