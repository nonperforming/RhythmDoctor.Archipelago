namespace RhythmDoctor.Archipelago.Modifiers.Archipelago.Traps;

internal class ScrambleHitsoundsTrap : ModifierPatch<ScrambleHitsoundsTrap>, IModifier, IArchipelagoModifier
{
  internal const string UID = $"{MyPluginInfo.PLUGIN_GUID}.mod.scrambleHitsounds";
  public string Uid => UID;
  public string LocalizationKey => "mods.archipelago.trap.scrambleHitsounds";
  public ModifierCompatibility Compatibility =>
    ModifierCompatibilityBuilder
      .GetDefaultBuilderForMod(this)
      .AddBlacklistedLevels(LevelExtensions.AllIntermissionLevels)
      .AddBlacklistedLevels(Level.Bitterness, Level.Blurred) // TODO: Handle custom beatsounds properly
      .AddBlacklistedLevels(Level.OrientalTechno) // TODO: Handle levels that don't use a rdlevel.
      .Build();
  public ModifierCapability[] Capabilities => [ModifierCapability.Hitsounds];

  public override Type[] PreviewPatches => [];
  public override Type[] ActivePatches => [typeof(ActivePatch)];

  public IScale Scale => BinaryScale.Instance;

  private static Dictionary<string, string> scrambled = new();

  // ReSharper disable once NullableWarningSuppressionIsUsed
  // Although this method is marked for P1 the only difference is the order of the sound effects
  private static readonly string[] hitsounds = LevelEvent_SetClapSounds.GetClapSoundsP1();

  public override void Active(float strength)
  {
    base.Active(strength);

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
    private static void ModifyClapSoundsDataPatch(RDLevelData __result)
    {
      Plugin.Logger.LogDebug("[Scramble Hitsounds] Modifying SetClapSounds level events");

      foreach (LevelEvent_Base levelEvent in __result.levelEvents)
      {
        if (levelEvent is LevelEvent_SetClapSounds setClapSounds)
        {
          Plugin.Logger.LogDebug("[Scramble Hitsounds] SetClapSounds in level events:");
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
