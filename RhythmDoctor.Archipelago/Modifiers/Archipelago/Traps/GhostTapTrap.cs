namespace RhythmDoctor.Archipelago.Modifiers.Archipelago.Traps;

/// <summary>
/// Take damage from ghost taps.
/// </summary>
/// <remarks>Adapted from https://github.com/Mysthaps/MyseIfRDPatches/blob/master/GhostTapMiss.cs</remarks>
internal class GhostTapTrap : ModifierPatch<GhostTapTrap>, IModifier, IArchipelagoModifier
{
  public string Uid => $"{MyPluginInfo.PLUGIN_GUID}.mod.ghostTap";
  public string LocalizationKey => "mods.archipelago.trap.ghostTap";
  public ModifierCompatibility Compatibility =>
    ModifierCompatibilityBuilder
      .GetDefaultBuilderForMod(this)
      .AddBlacklistedLevels(LevelExtensions.AllIntermissionLevels)
      .Build();
  public ModifierCapability[] Capabilities => [];

  public override Type[] PreviewPatches => [];
  public override Type[] ActivePatches => [typeof(ActivePatch)];

  public float GetScale(int num, out int consumed) => Scales.BinaryScale(num, out consumed);

  [HarmonyPatch]
  private static class ActivePatch
  {
    // ReSharper disable once RedundantDefaultMemberInitializer
    private static bool _endLevel = false;

    [HarmonyPatch(typeof(scnGame), nameof(scnGame.UpdateGameplayInput))]
    [HarmonyPostfix]
    private static void DamageGhostInputPatch(RDPlayer player, bool keyPressed, scnGame __instance)
    {
      if (_endLevel)
        return;

      if (
        !keyPressed
        || !__instance.spacebarOnNothing
        || __instance.levelIdentifier == nameof(Level.SongOfTheSea)
        || __instance.levelIdentifier == nameof(Level.SongOfTheSeaH)
      )
        return;

      scrConductor.PlayFeedback(
        GameSoundType.BigMistake,
        group: RDUtils.GetMixerGroup(player == RDPlayer.P1 ? "PlayerOneMistakes" : "PlayerTwoMistakes")
      );
      __instance.game.OnMistakeOrHeal(0f, 1f, null, false, player);
      scnGame.instance.FlashBorderFeedback(false);
    }

    [HarmonyPatch(typeof(Rankscreen), nameof(Rankscreen.AdvanceGameover))]
    [HarmonyPostfix]
    private static void DoNotDamageOnExitPatch()
    {
      _endLevel = true;
    }
  }
}
