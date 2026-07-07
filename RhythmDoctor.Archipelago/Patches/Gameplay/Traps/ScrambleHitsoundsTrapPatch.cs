namespace RhythmDoctor.Archipelago.Patches.Gameplay.Traps;

internal class ScrambleHitsoundsTrapPatch : ModifierPatch<ScrambleHitsoundsTrapPatch>, IModifier, IArchipelagoModifier
{
  public string Uid => $"{MyPluginInfo.PLUGIN_GUID}.mod.scrambleHitsounds";
  public string LocalizationKey => "mods.archipelago.trap.scrambleHitsounds";
  public ModifierCompatibility Compatibility =>
    ModifierCompatibilityBuilder
      .GetDefaultBuilderForMod(this)
      .AddBlacklistedLevels(LevelExtensions.AllIntermissionLevels)
      .AddBlacklistedLevels(Level.Bitterness) // TODO: Handle custom beatsounds properly and remove Bitterness from blacklist.
      .Build();
  public ModifierCapability[] Capabilities => [ModifierCapability.Characters];

  public override Type[] PreviewPatches => [];
  public override Type[] ActivePatches => [typeof(ActivePatch)];

  public float GetScale(int num, out int consumed) => Scales.BinaryScale(num, out consumed);

  private static Dictionary<string, string> scrambled = new();

  // ReSharper disable once NullableWarningSuppressionIsUsed
  // Although this method is marked for P1 the only difference is the order of the sound effects
  private static readonly string[] hitsounds = LevelEvent_SetClapSounds.GetClapSoundsP1();

  public void Active()
  {
    string[] randomizedOrder = (string[])hitsounds.Clone();

    Plugin.Random.Shuffle(randomizedOrder);

    for (int i = 0; i < randomizedOrder.Length; i++)
    {
      scrambled[hitsounds[i]] = randomizedOrder[i];
    }

    Plugin.Logger.LogDebug("[Scramble Hitsounds] Randomized hitsounds:");
    foreach ((string originalHitsound, string randomizedHitsound) in scrambled)
    {
      Plugin.Logger.LogDebug($"[Scramble Hitsounds]  {originalHitsound} -> {randomizedHitsound}");
    }
  }

  [HarmonyPatch(typeof(LevelBase))]
  private static class ActivePatch
  {
    [HarmonyPatch(nameof(LevelBase.DecodeLevelData))]
    [HarmonyPostfix]
    private static void ModifyCharacterDataPatch(RDLevelData __result)
    {
      Plugin.Logger.LogDebug("[Scramble Hitsounds] Modifying SetClapSounds level events");

      foreach (LevelEvent_Base levelEvent in __result.levelEvents)
      {
        if (levelEvent is LevelEvent_SetClapSounds setClapSounds)
        {
          Plugin.Logger.LogDebug("[Scramble Hitsounds] SetClapSounds in level events:");
          Plugin.Logger.LogWarning(setClapSounds.p1Sound?.filename);
          if (setClapSounds.p1Sound.HasValue)
          {
            Plugin.Logger.LogDebug(
              $"[Scramble Hitsounds]  P1  {setClapSounds.p1Sound.Value.filename} -> {scrambled[setClapSounds.p1Sound.Value.filename]}"
            );
            setClapSounds.p1Sound = new SoundDataStruct(scrambled[setClapSounds.p1Sound.Value.filename]);
          }
          if (setClapSounds.p2Sound.HasValue)
          {
            Plugin.Logger.LogDebug(
              $"[Scramble Hitsounds]  P2  {setClapSounds.p2Sound.Value.filename} -> {scrambled[setClapSounds.p2Sound.Value.filename]}"
            );
            setClapSounds.p2Sound = new SoundDataStruct(scrambled[setClapSounds.p2Sound.Value.filename]);
          }
          if (setClapSounds.cpuSound.HasValue)
          {
            Plugin.Logger.LogDebug(
              $"[Scramble Hitsounds]  CPU {setClapSounds.cpuSound.Value.filename} -> {scrambled[setClapSounds.cpuSound.Value.filename]}"
            );
            setClapSounds.cpuSound = new SoundDataStruct(scrambled[setClapSounds.cpuSound.Value.filename]);
          }
        }
      }
    }
  }
}
