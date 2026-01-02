namespace RhythmDoctor.Archipelago.Patches.Gameplay.Traps;

internal class ScrambleCharactersTrapPatch : ITrap
{
  // ReSharper disable once NullableWarningSuppressionIsUsed
  private Harmony _harmony = null!;

  private static Dictionary<Character, Character> scrambled = new();

  public static string name = "Scramble Characters";
  public string Name => name;
  public IEnumerable<Type> IncompatibleWithTraps => [typeof(ScrambleCharactersTrapPatch)];

  // FIXME: There should be no reason why Helping Hands is incompatible
  //        It appears theres more than 4 rows in a single room at a time, but we're handling MakeRow in level events.
  public IEnumerable<Level> IncompatibleWithLevels =>
    [Level.SongOfTheSea, Level.SongOfTheSeaH, Level.AthleteTherapy, Level.HelpingHands];

  private static readonly IReadOnlyCollection<Character> _disallowedCharacters =
  [
    Character.None,
    Character.Otto,
    Character.Custom,
    Character.Rodney,
    Character.DancingCouple,
    Character.Janitor,
    Character.Player,
    Character.BlankCPU,
    Character.AthletePhysio,
    Character.New,
    Character.Weightlifter,
  ];

  public void InQueue()
  {
    _harmony = new Harmony($"{Plugin.PATCH_ID_TRAP}.{nameof(ScrambleCharactersTrapPatch)}");
  }

  public void Active()
  {
    Character[] characters = (Character[])Enum.GetValues(typeof(Character));
    Character[] randomizedOrder = (Character[])characters.Clone();
    // Do not randomize these characters, as they will appear broken/as no character
    IReadOnlyList<Character> allowedCharacters = characters
      .Where(character => !_disallowedCharacters.Contains(character))
      .ToList();
    IList<Character> pool = ((Character[])allowedCharacters.ToArray().Clone()).ToList();
    Plugin.Random.Shuffle(randomizedOrder);

    for (int i = 0; i < randomizedOrder.Length; i++)
    {
      Character originalCharacter = characters[i];
      // Unscramble forbidden characters
      if (_disallowedCharacters.Contains(originalCharacter))
      {
        scrambled[originalCharacter] = originalCharacter;
        continue;
      }

      Character randomizeTo = randomizedOrder[i];

      if (_disallowedCharacters.Contains(randomizeTo))
      {
        int randomizeIndex = Plugin.Random.Next(0, pool.Count);
        randomizeTo = allowedCharacters[randomizeIndex];
        pool.RemoveAt(randomizeIndex);
      }

      scrambled[originalCharacter] = randomizeTo;
    }

    Plugin.Logger.LogDebug("[Scramble Characters] Randomized characters:");
    foreach ((Character originalCharacter, Character randomizedCharacter) in scrambled)
    {
      Plugin.Logger.LogDebug($"[Scramble Characters]  {originalCharacter} -> {randomizedCharacter}");
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
      Plugin.Logger.LogDebug("[Scramble Characters] Modifying MakeRow and ChangeCharacter level events");

      foreach (LevelEvent_Base levelEvent in __result.levelEvents)
      {
        if (levelEvent is LevelEvent_CallCustomMethod callCustomMethod)
        {
          if (!callCustomMethod.methodName.StartsWith("ChangeCharacter"))
          {
            continue;
          }

          Plugin.Logger.LogDebug($"[Scramble Characters] ChangeCharacter custom method: {callCustomMethod.methodName}");

          string changeToString = callCustomMethod
            .methodName.Replace("ChangeCharacterSmooth(str:", "", StringComparison.Ordinal)
            .Replace("ChangeCharacter(str:", "", StringComparison.Ordinal)
            .Split(",")[0];
          Character changeTo = Enum.Parse<Character>(changeToString);
          Character randomized = scrambled[changeTo];

          callCustomMethod.methodName.Replace(changeToString, randomized.ToString(), StringComparison.Ordinal);
          Plugin.Logger.LogDebug($"[Scramble Characters] Character changed to: {randomized}");
        }
        else if (levelEvent is LevelEvent_ChangeCharacter changeCharacter)
        {
          Plugin.Logger.LogDebug($"[Scramble Characters] ChangeCharacter event: {changeCharacter.character}");
          changeCharacter.character = scrambled[changeCharacter.character];
          Plugin.Logger.LogDebug($"[Scramble Characters] Character changed to: {changeCharacter.character}");
        }
        else if (levelEvent is LevelEvent_MakeRow makeRow)
        {
          Plugin.Logger.LogDebug(
            $"[Scramble Characters] MakeRow in level events: {makeRow.character} -> {scrambled[makeRow.character]}"
          );
          // Some levels such as X-0 Helping Hands use the MakeRow event manually
          makeRow.character = scrambled[makeRow.character];
        }
      }
    }

    [HarmonyPatch(typeof(RDInk), nameof(RDInk.ParsePortrait))]
    [HarmonyPrefix]
    private static void ModifyInkPortraitPatch(ref string fullName)
    {
      string character = fullName.Split("_")[0];
      if (Enum.TryParse(character, out Character toRandomize))
      {
        Character randomized = scrambled[toRandomize];
        string scrambledTo = randomized.ToString();
        Plugin.Logger.LogDebug($"[Scramble Characters] Modifying portrait from {toRandomize} to {scrambledTo}");
        fullName = fullName.Replace(character, scrambledTo);
      }
    }
  }
}
