namespace RhythmDoctor.Archipelago.Patches.Gameplay.Traps;

internal class ScrambleCharactersTrapPatch : ITrap
{
  // FIXME: Narration will break

  // ReSharper disable once NullableWarningSuppressionIsUsed
  private Harmony _harmony = null!;

  private static Dictionary<Character, Character> scrambled = new();

  public static string name = "Scramble Characters";
  public string Name => name;
  public IEnumerable<Type> IncompatibleWithTraps => [typeof(ScrambleCharactersTrapPatch)];

  public IEnumerable<Level> IncompatibleWithLevels => [Level.SongOfTheSea, Level.SongOfTheSeaH, Level.AthleteTherapy];

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
    // Do not scramble these characters, as they will appear broken/no character
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

    Plugin.Logger.LogDebug($"[{nameof(ScrambleCharactersTrapPatch)}] Randomized characters:");
    foreach ((Character originalCharacter, Character randomizedCharacter) in scrambled)
    {
      Plugin.Logger.LogDebug($"[{nameof(ScrambleCharactersTrapPatch)}]  {originalCharacter} -> {randomizedCharacter}");
    }
    _harmony.PatchAll(typeof(ActivePatch));
  }

  public void ActiveEnd()
  {
    _harmony.UnpatchSelf();
  }

  [HarmonyPatch]
  private static class ActivePatch
  {
    [HarmonyPatch(typeof(LevelBase), nameof(LevelBase.DecodeLevelData))]
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

          Plugin.Logger.LogDebug(
            $"[{nameof(ScrambleCharactersTrapPatch)}] ChangeCharacter custom method: {callCustomMethod.methodName}"
          );

          string changeToString = callCustomMethod
            .methodName.Replace("ChangeCharacterSmooth(str:", "", StringComparison.Ordinal)
            .Replace("ChangeCharacter(str:", "", StringComparison.Ordinal)
            .Split(",")[0];
          Character changeTo = Enum.Parse<Character>(changeToString);
          Character randomized = scrambled[changeTo];

          callCustomMethod.methodName = callCustomMethod.methodName.Replace(
            changeToString,
            randomized.ToString(),
            StringComparison.Ordinal
          );
          Plugin.Logger.LogDebug($"[{nameof(ScrambleCharactersTrapPatch)}] Character changed to: {randomized}");
        }
      }
    }

    [HarmonyPatch(typeof(scnGame), nameof(scnGame.MakeRow))]
    [HarmonyPrefix]
    private static void ModifyMakeRowPatch(ref Character character)
    {
      Plugin.Logger.LogDebug(
        $"[{nameof(ScrambleCharactersTrapPatch)}] Modifying MakeRow method from {character} to {scrambled[character]}"
      );
      character = scrambled[character];
    }

    [HarmonyPatch(typeof(scrChar), nameof(scrChar.ChangeCharacter))]
    [HarmonyPrefix]
    private static void ModifyChangeCharacterPatch(ref Character newChar)
    {
      Plugin.Logger.LogDebug(
        $"[{nameof(ScrambleCharactersTrapPatch)}] Modifying ChangeCharacter method from {newChar} to {scrambled[newChar]}"
      );
      newChar = scrambled[newChar];
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
