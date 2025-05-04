namespace RhythmDoctor.Archipelago.Patches.Gameplay.Traps;

// Adapted from https://github.com/Mysthaps/MyseIfRDPatches/blob/master/GhostTapMiss.cs
static class GhostTapTrapPatch
{
  static bool EndLevel = false;

  [HarmonyPostfix]
  [HarmonyPatch(typeof(HUD), nameof(HUD.AdvanceGameover))]
  public static void DoNotDamageOnExitPatch()
  {
    EndLevel = true;
  }

  [HarmonyPostfix]
  [HarmonyPatch(typeof(scnGame), nameof(scnGame.UpdateGameplayInput))]
  public static void DamageGhostInputPatch(RDPlayer player, bool keyPressed, scnGame __instance)
  {
    if (EndLevel)
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

    scrConductor.PlayImmediately(
      GameSoundType.BigMistake,
      group: RDUtils.GetMixerGroup((player == RDPlayer.P1 ? "PlayerOneMistakes" : "PlayerTwoMistakes")),
      pan: (GC.twoPlayerMode ? RDUtils.OverridePanFor2P(player, 0.0f) : 0.0f)
    );
    __instance.game.OnMistakeOrHeal(0f, 1f, null, false, player);
    RDBase.Vfx.FlashBorderFeedback(false);
  }
}
