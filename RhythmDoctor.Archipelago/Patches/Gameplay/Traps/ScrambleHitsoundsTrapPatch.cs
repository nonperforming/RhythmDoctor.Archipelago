namespace RhythmDoctor.Archipelago.Patches.Gameplay.Traps;

internal class ScrambleHitsoundsTrapPatch : ITrap
{
  // ReSharper disable once NullableWarningSuppressionIsUsed
  private Harmony _harmony = null!;

  private static Dictionary<string, string> scrambled = new();

  // ReSharper disable once NullableWarningSuppressionIsUsed
  // This will be populated in InQueue if need be.
  private static string[] hitsounds = null!;

  public string Name => "Scramble Hitsounds";
  public IEnumerable<Type> IncompatibleWithTraps => [typeof(ScrambleHitsoundsTrapPatch)];

  // FIXME: Handle custom beatsounds properly and remove Bitterness from blacklist.
  public IEnumerable<Level> IncompatibleWithLevels =>
    [Level.SongOfTheSea, Level.SongOfTheSeaH, Level.AthleteTherapy, Level.Bitterness];

  public void InQueue()
  {
    // Although this method is marked for P1 the only difference is the order of the sound effects
    // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
    hitsounds ??= LevelEvent_SetClapSounds.GetClapSoundsP1();
    _harmony = new Harmony($"{Plugin.PATCH_ID_TRAP}.{nameof(ScrambleHitsoundsTrapPatch)}");
  }

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
    _harmony.PatchAll(typeof(ActivePatch));
  }

  public void ActiveEnd()
  {
    _harmony.UnpatchSelf();
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
