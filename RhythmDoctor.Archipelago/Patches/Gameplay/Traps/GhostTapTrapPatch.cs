namespace RhythmDoctor.Archipelago.Patches.Gameplay.Traps;

// Adapted from https://github.com/Mysthaps/MyseIfRDPatches/blob/master/GhostTapMiss.cs
class GhostTapTrapPatch : ITrap
{
  // ReSharper disable once NullableWarningSuppressionIsUsed
  private Harmony harmony = null!;

  public string Name => "Ghost Tap";
  public Type[] IncompatibleWithTraps => [typeof(GhostTapTrapPatch)];

  public void InQueue()
  {
    harmony = new($"{Plugin.PATCH_ID_TRAP}.{nameof(GhostTapTrapPatch)}");
  }

  public void Active()
  {
    harmony.PatchAll(typeof(ActivePatch));
  }

  public void ActiveEnd()
  {
    harmony.UnpatchSelf();
  }

  private static class ActivePatch
  {
    // ReSharper disable once RedundantDefaultMemberInitializer
    private static bool _endLevel = false;

    [HarmonyPatch(typeof(scnGame), nameof(scnGame.UpdateGameplayInput))]
    [HarmonyPostfix]
    static void DamageGhostInputPatch(RDPlayer player, bool keyPressed, scnGame __instance)
    {
      if (_endLevel)
        return;

      if (
        !keyPressed
        || !__instance.spacebarOnNothing
        || __instance.levelIdentifier == nameof(Level.SongOfTheSea)
        || __instance.levelIdentifier == nameof(Level.SongOfTheSeaH)
      )
      {
        return;
      }

      scrConductor.PlayFeedback(
        GameSoundType.BigMistake,
        group: RDUtils.GetMixerGroup((player == RDPlayer.P1 ? "PlayerOneMistakes" : "PlayerTwoMistakes"))
      );
      __instance.game.OnMistakeOrHeal(0f, 1f, null, false, player);
      RDBase.Vfx.FlashBorderFeedback(false);
    }

    [HarmonyPatch(typeof(HUD), nameof(HUD.AdvanceGameover))]
    [HarmonyPostfix]
    static void DoNotDamageOnExitPatch()
    {
      _endLevel = true;
    }
  }
}
