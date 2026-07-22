namespace RhythmDoctor.Archipelago.Patches.Gameplay.Traps;

internal class ScrambleBeatsoundsTrapPatch : ModifierPatch<ScrambleBeatsoundsTrapPatch>, IModifier, IArchipelagoModifier
{
  public string Uid => $"{MyPluginInfo.PLUGIN_GUID}.mod.scrambleBeatsounds";
  public string LocalizationKey => "mods.archipelago.trap.scrambleBeatsounds";
  public ModifierCompatibility Compatibility =>
    ModifierCompatibilityBuilder
      .GetDefaultBuilderForMod(this)
      .AddBlacklistedLevels(LevelExtensions.AllIntermissionLevels)
      .Build();
  public ModifierCapability[] Capabilities => [ModifierCapability.Beatsounds];

  public override Type[] PreviewPatches => [];
  public override Type[] ActivePatches => [typeof(ActivePatch)];

  public float GetScale(int num, out int consumed) => Scales.BinaryScale(num, out consumed);

  private static readonly Dictionary<SoundEffect, SoundEffect> scrambled = new();

  public override void Active(float strength)
  {
    base.Active(strength);

    SoundEffect[] randomizedOrder = (SoundEffect[])RDEditorConstants.BeatSounds.Clone();

    Plugin.Random.Shuffle(randomizedOrder);

    for (int i = 0; i < randomizedOrder.Length; i++)
    {
      SoundEffect originalBeatsound = RDEditorConstants.BeatSounds[i];
      SoundEffect randomizeTo = randomizedOrder[i];

      if (randomizeTo == SoundEffect.None)
      {
        int num = Plugin.Random.Next(0, RDEditorConstants.BeatSounds.Length);
        if (num == Array.IndexOf(RDEditorConstants.BeatSounds, SoundEffect.None))
        {
          num++;
        }
        randomizeTo = RDEditorConstants.BeatSounds[num];
      }

      scrambled[originalBeatsound] = randomizeTo;
    }

    Plugin.Logger.LogDebug("Randomized beatsounds:");
    foreach ((SoundEffect originalBeatsound, SoundEffect randomizedBeatsound) in scrambled)
    {
      Plugin.Logger.LogDebug($"  {originalBeatsound} -> {randomizedBeatsound}");
    }
  }

  [HarmonyPatch(typeof(LevelBase))]
  private static class ActivePatch
  {
    [HarmonyPatch(nameof(LevelBase.DecodeLevelData))]
    [HarmonyPostfix]
    private static void ModifyCharacterDataPatch(RDLevelData __result)
    {
      Plugin.Logger.LogDebug("Scramble Beatsounds: Modifying MakeRow and SetBeatSound level events");

      foreach (LevelEvent_MakeRow row in __result.rows)
      {
        SoundEffect originalSound = Enum.Parse<SoundEffect>(
          row.pulseSound.filename.Replace("snd", "", StringComparison.Ordinal)
        );
        SoundEffect randomizedSound = scrambled[originalSound];

        Plugin.Logger.LogDebug($"MakeRow in rows: {originalSound} -> {randomizedSound}");
        row.pulseSound.filename = randomizedSound.ToString();
      }

      foreach (LevelEvent_Base levelEvent in __result.levelEvents)
      {
        if (levelEvent is LevelEvent_SetBeatSound setBeatSound)
        {
          SoundEffect originalSound = Enum.Parse<SoundEffect>(setBeatSound.sound.filename);
          SoundEffect randomizedSound = scrambled[originalSound];
          Plugin.Logger.LogDebug($"SetBeatSound in level events: {originalSound} -> {randomizedSound}");
          setBeatSound.sound = new SoundDataStruct(
            randomizedSound.ToString().Replace("snd", "", StringComparison.Ordinal)
          );
        }
      }
    }
  }
}
