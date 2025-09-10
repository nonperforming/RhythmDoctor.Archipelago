namespace RhythmDoctor.Archipelago.Patches.Gameplay.Traps;

internal class ScrambleBeatsoundsTrapPatch : ITrap
{
  // ReSharper disable once NullableWarningSuppressionIsUsed
  private Harmony harmony = null!;

  private static Dictionary<SoundEffect, SoundEffect> scrambled = new();

  public string Name => "Scramble Beatsounds";
  public IEnumerable<Type> IncompatibleWithTraps => [typeof(ScrambleBeatsoundsTrapPatch)];
  public IEnumerable<Level> IncompatibleWithLevels => [Level.SongOfTheSea, Level.SongOfTheSeaH, Level.AthleteTherapy];

  public void InQueue()
  {
    harmony = new($"{Plugin.PATCH_ID_TRAP}.{nameof(ScrambleCharactersTrapPatch)}");
  }

  public void Active()
  {
    SoundEffect[] randomizedOrder = (SoundEffect[])RDEditorConstants.PulseSounds.Clone();

    Random random = new();
    random.Shuffle(randomizedOrder);

    for (int i = 0; i < randomizedOrder.Length; i++)
    {
      SoundEffect originalBeatsound = RDEditorConstants.PulseSounds[i];
      SoundEffect randomizeTo = randomizedOrder[i];

      if (randomizeTo == SoundEffect.None)
      {
        int num = random.Next(0, RDEditorConstants.PulseSounds.Length);
        if (num == Array.IndexOf(RDEditorConstants.PulseSounds, SoundEffect.None))
        {
          num++;
        }
        randomizeTo = RDEditorConstants.PulseSounds[num];
      }

      scrambled[originalBeatsound] = randomizeTo;
    }

    Plugin.Logger.LogDebug("Randomized beatsounds:");
    foreach ((SoundEffect originalBeatsound, SoundEffect randomizedBeatsound) in scrambled)
    {
      Plugin.Logger.LogDebug($"  {originalBeatsound} -> {randomizedBeatsound}");
    }
    harmony.PatchAll(typeof(Patch));
  }

  public void ActiveEnd()
  {
    harmony.UnpatchSelf();
  }

  [HarmonyPatch(typeof(RDLevelData))]
  private static class Patch
  {
    [HarmonyPatch(nameof(RDLevelData.Decode))]
    [HarmonyPostfix]
    static void ModifyCharacterDataPatch(RDLevelData __result)
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
