#if DEBUG
namespace RhythmDoctor.Archipelago.Debug.Patches;

// ReSharper disable once CommentTypo
/// <summary>
/// Logs RDBaseDllDummy.printe,
/// RDBaseDllDummy.printe_frame,
/// RDBaseDllDummy.printef,
/// RDBaseDllDummy.printem,
/// RDBaseDllDummy.printes,
/// RDBaseDllDummy.printesw,
/// RDBaseDllDummy.printw method calls
/// </summary>
[HarmonyPatch(typeof(RDBaseDllDummy))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal static class LogRDPrintPatch
{
  [HarmonyPatch(nameof(RDBaseDllDummy.printe))]
  [HarmonyPrefix]
  internal static void E(object o)
  {
    Plugin.Logger?.LogDebug(o);
  }

  [HarmonyPatch(nameof(RDBaseDllDummy.printe_frame))]
  [HarmonyPrefix]
  internal static void EFrame(object o)
  {
    Plugin.Logger?.LogDebug(o);
  }

  [HarmonyPatch(nameof(RDBaseDllDummy.printef))]
  [HarmonyPrefix]
  internal static void EF(object o)
  {
    Plugin.Logger?.LogDebug(o);
  }

  [HarmonyPatch(nameof(RDBaseDllDummy.printem))]
  [HarmonyPrefix]
  internal static void EM(object o)
  {
    Plugin.Logger?.LogDebug(o);
  }

  [HarmonyPatch(nameof(RDBaseDllDummy.printes))]
  [HarmonyPrefix]
  internal static void ES(object o)
  {
    Plugin.Logger?.LogDebug(o);
  }

  [HarmonyPatch(nameof(RDBaseDllDummy.printesw))]
  [HarmonyPrefix]
  internal static void ESW(object o)
  {
    Plugin.Logger?.LogDebug(o);
  }

  [HarmonyPatch(nameof(RDBaseDllDummy.printw))]
  [HarmonyPrefix]
  internal static void W(object o)
  {
    Plugin.Logger?.LogDebug(o);
  }
}
#endif
