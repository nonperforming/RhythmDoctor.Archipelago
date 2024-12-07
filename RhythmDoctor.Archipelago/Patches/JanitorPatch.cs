namespace RhythmDoctor.Archipelago.Patches;

/// <summary>
/// Patches related to the Janitor
/// </summary>
[HarmonyPatch(typeof(scnLevelSelect))]
internal static class JanitorPatch
{
  /// <summary>
  ///
  /// </summary>
  /// <param name="__instance">Instance of <see cref="scnLevelSelect"/> that ran <see cref="scnLevelSelect.PerformEntityAction"/></param>
  /// <param name="__runOriginal">Whether to run the original method or not. This will be set to false if interacting with the Janitor.</param>
  [HarmonyPatch(nameof(scnLevelSelect.PerformEntityAction))]
  [HarmonyPrefix]
  static void InteractJanitorPatch(scnLevelSelect __instance, ref bool __runOriginal)
  {
    SelectableObject? currentSelectableObject = __instance.selectedEntity as SelectableObject;

    if (currentSelectableObject == null)
    {
      Plugin.Logger?.LogWarning("Current selectable object is null");
      return;
    }
    if (currentSelectableObject.action != "talkToJanitor")
    {
      Plugin.Logger?.LogDebug("Current selectable object's action is not talkToJanitor");
      return;
    }

    // We have selected a Janitor object.
    // Load the custom level ward, add our custom options, and delete the old options
    //__runOriginal = false;
    // TODO: Implement janitor menu patch. (additional comment to show on TODO plugins)
    Plugin.Logger?.LogError("TODO: Implement janitor menu patch");
  }

  /// <summary>
  /// Prevent <see cref="scnLevelSelect.PlaceJanitor"/> from running and hiding Janitors.
  /// </summary>
  /// <param name="__runOriginal">Whether to run the original method or not. This will always be set to <c>false</c>.</param>
    [HarmonyPatch(nameof(scnLevelSelect.PlaceJanitor))]
  [HarmonyPrefix]
  static void PlaceJanitorPatch(ref bool __runOriginal)
  {
    Plugin.Logger?.LogDebug("Bypassing PlaceJanitor");
    __runOriginal = false;
  }

  /// <summary>
  /// Prevent <see cref="scnLevelSelect.HideJanitor"/> from running and hiding Janitors.
  /// This patch is here in case <see cref="PlaceJanitorPatch"/> somehow fails to skip, due to a game update,
  /// or from other plugins hiding the Janitors by calling this method directly.
  /// This patch should never run under normal circumstances.
  /// </summary>
  /// <param name="__runOriginal">Whether to run the original method or not. This will always be set to <c>false</c>.</param>
  [HarmonyPatch(nameof(scnLevelSelect.HideJanitor))]
  [HarmonyPrefix]
  static void HideJanitorPatch(ref bool __runOriginal)
  {
    Plugin.Logger?.LogWarning("Bypassing HideJanitor. This should never be called assuming PlaceJanitor was bypassed!");
    __runOriginal = false;
  }
}
