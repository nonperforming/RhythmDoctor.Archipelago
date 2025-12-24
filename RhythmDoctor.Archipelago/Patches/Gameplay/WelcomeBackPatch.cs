namespace RhythmDoctor.Archipelago.Patches.Gameplay;

[HarmonyPatch(typeof(Conditional_Custom))]
internal static class WelcomeBackPatch
{
  [HarmonyPatch(nameof(Conditional_Custom.Check))]
  [HarmonyPrefix]
  private static void DoNotSkipToBossFightPatch(
    Conditional_Custom __instance,
    ref bool __runOriginal,
    ref bool __result
  )
  {
    if (Enum.TryParse(scnGame.internalIdentifier, out Level level) && level == Level.EdegaRave)
    {
      __runOriginal = __instance.customExpression != $"passedLevel({nameof(Level.EdegaRave)})";
      if (!__runOriginal)
      {
        Plugin.Logger.LogInfo("Forcing 6-2 to 6-X transition off");
        __result = false; // conditional is inverted
      }
    }
  }
}
