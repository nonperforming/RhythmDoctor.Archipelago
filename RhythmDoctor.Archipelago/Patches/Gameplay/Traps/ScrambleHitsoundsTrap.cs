using System.Reflection;

namespace RhythmDoctor.Archipelago.Patches.Gameplay.Traps;

internal class ScrambleHitsoundsTrapPatch : ITrap
{
  // ReSharper disable once NullableWarningSuppressionIsUsed
  private Harmony _harmony = null!;

  private static Dictionary<string, string> scrambled = new();

  // ReSharper disable once NullableWarningSuppressionIsUsed
  // This will be populated in InQueue if need be.
  private static IReadOnlyList<string> hitsounds = null!;

  public string Name => "Scramble Hitsounds";
  public IEnumerable<Type> IncompatibleWithTraps => [typeof(ScrambleHitsoundsTrapPatch)];
  public IEnumerable<Level> IncompatibleWithLevels => [Level.SongOfTheSea, Level.SongOfTheSeaH, Level.AthleteTherapy];

  public void InQueue()
  {
    // Populate Hitsounds
    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
    if (hitsounds == null)
    {
      // These all exist in the game.
      // ReSharper disable NullableWarningSuppressionIsUsed
      PropertyInfo p1Sound = typeof(LevelEvent_SetClapSounds).GetProperty(nameof(LevelEvent_SetClapSounds.p1Sound))!;
      PropertyInfo p2Sound = typeof(LevelEvent_SetClapSounds).GetProperty(nameof(LevelEvent_SetClapSounds.p2Sound))!;
      PropertyInfo cpuSound = typeof(LevelEvent_SetClapSounds).GetProperty(nameof(LevelEvent_SetClapSounds.cpuSound))!;
      // ReSharper restore NullableWarningSuppressionIsUsed

      SoundAttribute[] soundAttributes =
      [
        (SoundAttribute)p1Sound.GetCustomAttribute(typeof(SoundAttribute)),
        (SoundAttribute)p2Sound.GetCustomAttribute(typeof(SoundAttribute)),
        (SoundAttribute)cpuSound.GetCustomAttribute(typeof(SoundAttribute)),
      ];

      hitsounds = soundAttributes.SelectMany(soundAttribute => soundAttribute.options).Distinct().ToList();
    }

    _harmony = new Harmony($"{Plugin.PATCH_ID_TRAP}.{nameof(ScrambleHitsoundsTrapPatch)}");
  }

  public void Active()
  {
    string[] randomizedOrder = hitsounds.ToArray();

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
