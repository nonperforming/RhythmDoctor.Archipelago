namespace RhythmDoctor.Archipelago.Patches.Gameplay.Traps;

internal class ScrambleCharactersTrapPatch : ITrap
{
  // ReSharper disable once NullableWarningSuppressionIsUsed
  private Harmony harmony = null!;

  private static Dictionary<Character, Character> scrambled = new();

  public string Name => "Scramble Characters";
  public IEnumerable<Type> IncompatibleWithTraps => [typeof(ScrambleCharactersTrapPatch)];
  public IEnumerable<Level> IncompatibleWithLevels =>
    [Level.SongOfTheSea, Level.SongOfTheSeaH, Level.AthleteTherapy, Level.HelpingHands];

  public void InQueue()
  {
    harmony = new($"{Plugin.PATCH_ID_TRAP}.{nameof(ScrambleCharactersTrapPatch)}");
  }

  public void Active()
  {
    Character[] characters = (Character[])Enum.GetValues(typeof(Character));
    Character[] randomizedOrder = (Character[])characters.Clone();
    // Do not randomize these characters, as they will appear broken/as no character
    IReadOnlyCollection<Character> disallowedCharacters =
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
    IReadOnlyList<Character> allowedCharacters = characters
      .Where(character => !disallowedCharacters.Contains(character))
      .ToList();
    IList<Character> pool = ((Character[])(allowedCharacters.ToArray().Clone())).ToList();
    Random random = new();
    random.Shuffle(randomizedOrder);

    for (int i = 0; i < randomizedOrder.Length; i++)
    {
      Character originalCharacter = characters[i];
      Character randomizeTo = randomizedOrder[i];

      if (disallowedCharacters.Contains(randomizeTo))
      {
        int randomizeIndex = random.Next(0, pool.Count);
        randomizeTo = allowedCharacters[randomizeIndex];
        pool.RemoveAt(randomizeIndex);
      }

      scrambled[originalCharacter] = randomizeTo;
    }

    Plugin.Logger.LogDebug("Randomized characters:");
    foreach ((Character originalCharacter, Character randomizedCharacter) in scrambled)
    {
      Plugin.Logger.LogDebug($"  {originalCharacter} -> {randomizedCharacter}");
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
      Plugin.Logger.LogDebug("Scramble Characters: Modifying MakeRow and ChangeCharacter level events");

      foreach (LevelEvent_MakeRow row in __result.rows)
      {
        Plugin.Logger.LogDebug($"MakeRow in rows: {row.character} -> {scrambled[row.character]}");
        row.character = scrambled[row.character];
      }

      foreach (LevelEvent_Base levelEvent in __result.levelEvents)
      {
        if (levelEvent is LevelEvent_CallCustomMethod callCustomMethod)
        {
          if (!callCustomMethod.methodName.StartsWith("ChangeCharacter"))
            continue;

          Plugin.Logger.LogDebug($"ChangeCharacter custom method: {callCustomMethod.methodName}");

          string changeToString = callCustomMethod
            .methodName.Replace("ChangeCharacterSmooth(str:", "", StringComparison.Ordinal)
            .Replace("ChangeCharacter(str:", "", StringComparison.Ordinal)
            .Split(",")[0];
          Character changeTo = Enum.Parse<Character>(changeToString);
          Character randomized = scrambled[changeTo];

          Plugin.Logger.LogDebug($"Character changed to: {randomized}");

          callCustomMethod.methodName.Replace(changeToString, randomized.ToString(), StringComparison.Ordinal);
        }
        else if (levelEvent is LevelEvent_MakeRow makeRow)
        {
          Plugin.Logger.LogDebug($"MakeRow in level events: {makeRow.character} -> {scrambled[makeRow.character]}");
          // Some levels such as X-0 Helping Hands use the MakeRow event manually
          makeRow.character = scrambled[makeRow.character];
        }
      }
    }
  }
}
