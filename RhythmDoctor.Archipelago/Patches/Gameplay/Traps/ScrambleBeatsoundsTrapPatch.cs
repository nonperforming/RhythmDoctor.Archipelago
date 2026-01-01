namespace RhythmDoctor.Archipelago.Patches.Gameplay.Traps;

internal class ScrambleBeatsoundsTrapPatch : ITrap
{
  // ReSharper disable once NullableWarningSuppressionIsUsed
  private Harmony _harmony = null!;

  private static Dictionary<SoundEffect, SoundEffect> scrambled = new();

  public string Name => "Scramble Beatsounds";
  public IEnumerable<Type> IncompatibleWithTraps => [typeof(ScrambleBeatsoundsTrapPatch)];
  public IEnumerable<Level> IncompatibleWithLevels => [Level.SongOfTheSea, Level.SongOfTheSeaH, Level.AthleteTherapy];

  public void InQueue()
  {
    _harmony = new Harmony($"{Plugin.PATCH_ID_TRAP}.{nameof(ScrambleBeatsoundsTrapPatch)}");
  }

  public void Active()
  {
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
    _harmony.PatchAll(typeof(Patch));
  }

  public void ActiveEnd()
  {
    _harmony.UnpatchSelf();
  }

  [HarmonyPatch(typeof(LevelBase))]
  private static class Patch
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
