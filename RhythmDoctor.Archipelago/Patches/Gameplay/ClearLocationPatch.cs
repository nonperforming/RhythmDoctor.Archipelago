namespace RhythmDoctor.Archipelago.Patches.Gameplay;

// This patch should only be applied after the Client is created.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

[HarmonyPatch]
internal static class ClearLocationPatch
{
  private static Dictionary<long, ScoutedItemInfo> ItemsToSend = new();

  // TODO: Hack for ShowSentItemsPatch as ShowAndSaveRank is called before we show the rank description text.
  //       Someone should probably clean this up.
  private static long[] JustSentLocations = [];

  [HarmonyPatch(typeof(LevelBase), nameof(LevelBase), MethodType.Constructor)]
  [HarmonyPrefix]
#pragma warning disable HARMONIZE001
  private static void ScoutItemsSentPatch(LevelBase __instance)
#pragma warning restore HARMONIZE001
  {
    Plugin.Logger.LogDebug("Scouting locations to send");
    ItemsToSend.Clear();
    JustSentLocations = [];

    // Get the locations we will clear so we can show the user
    // the locations they clear when they pass the level (replace the rank text)
    if (!Enum.TryParse(scnGame.internalIdentifier, out Level level))
    {
      Plugin.Logger.LogError($"Couldn't find Level. Level identifier: {scnGame.internalIdentifier}");
      Plugin.Client.TrapManager.ClearActiveTraps(false);
      return;
    }

    IEnumerable<long> ids;
    if (level == Level.Lesmis && __instance.dogMode)
    {
      // ReSharper disable once NullableWarningSuppressionIsUsed
      Dictionary<string, long> extraLocations = ((BossStage)Bindings.LevelToStage[Level.Lesmis]).ExtraLocations!;
      ids = new List<long>([extraLocations["dog_perfect"], extraLocations["dog_clear"]]);
    }
    else
    {
      // Guaranteed to be ordered from the highest to lowest rank's locations.
      ids = Bindings.LevelToStage[level].GetLocationsToClear(Rank.S);
    }

    Plugin.Instance.StartCoroutine(ScoutLocationChecks(ids.ToArray()));
  }

  /// <summary>
  /// Send relevant locations (and end goal if applicable) when clearing a level.
  /// </summary>
  /// <seealso cref="MiracleDefibrillatorClearLocationPatch"/>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if end goal is not valid.</exception>
  [HarmonyPatch(typeof(Rankscreen), nameof(Rankscreen.ShowAndSaveRank))]
  [HarmonyPrefix]
  private static void CustomClearLocationPatch(bool bossLevelFailed, bool onlySavePersistence, Rankscreen __instance)
  {
    // Is onlySavePersistence is currently only used in custom levels?
    // "there's a function for custom levels to skip the rank text [rank screen] at the end"
    // "intended to be used to make your own custom rank screen"
    // Relevant scripts:
    // global::Rankscreen.AdvanceGameover(bool) L41:
    // `bool skipRankText = base.game.currentLevel.skipRankText;`
    // global::Rankscreen.AdvanceGameover(bool) L48:
    // `ShowAndSaveRank(bossLevelFailed: false, skipRankText);`
    // TODO: Boss levels and said "custom levels" will overwrite virtual LevelBase.ShowGameOver
    //       Check for Rankscreen.base.game.currentLevel.customGameover!!! When is this case applicable?

    Level level = GetCurrentLevel();
    Rank rank = scnGame.instance.currentLevel.GetRankFromMistakes();
    SendLocations(level, rank, bossLevelFailed);
  }

  [HarmonyPatch(typeof(RhythmWeightlifter.Level), nameof(RhythmWeightlifter.Level.GetRank))]
  [HarmonyPostfix]
  private static void RhythmWeightlifterClearLocationPatch(string __result)
  {
    if (__result == "-")
    {
      // We haven't actually cleared the level yet.
      return;
    }
    // TODO: Show what item we have sent out somehow.
    Plugin.Client.Session.Locations.CompleteLocationChecks(
      Bindings.RhythmWeightlifterStageToLocationID[RhythmWeightlifter.scnRhythmWeightlifter.gameInstance.LevelIndex]
    );
  }

  [HarmonyPatch(typeof(Rankscreen), nameof(Rankscreen.ShowRankDescription))]
  [HarmonyPostfix]
  private static void ShowSentItemsPatch(Rankscreen __instance)
  {
    // TODO: Need to check if this works with narration.
    if (!ItemsToSend.Any())
    {
      Plugin.Logger.LogWarning("Couldn't get items sent, possibly due to a network issue.");
      __instance.description.text = "<color=red>Couldn't get items sent.</color>";
    }
    else
    {
      Rank rank = scnGame.instance.currentLevel.GetRankFromMistakes();
      IEnumerable<long> ids = GetStageLocationIDsToClear(GetCurrentLevel(), rank);
      long[] newLocations = ids.Where(id =>
          !Plugin.Client.Session.Locations.AllLocationsChecked.Contains(id) || JustSentLocations.Contains(id)
        )
        .ToArray();

      Plugin.Logger.LogDebug($"IDs: {string.Join(", ", ids)} (of which {string.Join(", ", newLocations)} are new)");

      // TODO: It'd be nice to show which rank sent what item.

      if (!ids.Any())
      {
        __instance.description.text = "Didn't find anything.";
        return;
      }
      else if (newLocations.Length == 0)
      {
        __instance.description.text = "Didn't find anything new.";
        return;
      }

      // We can't use <size> tags, as we will get this error:
      //  Font 'RDLatinFontPoint (UnityEngine.Font)' is not dynamic, which is required to override its size
      __instance.description.text = "";
      __instance.description.fontSize = 10;
      foreach (long id in newLocations)
      {
        if (!ItemsToSend.TryGetValue(id, out ScoutedItemInfo? itemInfo))
        {
          Plugin.Logger.LogWarning(
            $"Could not get scouted item information for location ID {id}. "
              + "This is normal if this ID is tied to a Perfect rank, "
              + "and Perfect locations are disabled."
          );
          continue;
        }

        string color = "silver"; // Filler
        if (itemInfo.Flags.HasFlag(ItemFlags.Advancement) || itemInfo.Flags.HasFlag(ItemFlags.NeverExclude))
        {
          color = "yellow";
        }
        else if (itemInfo.Flags.HasFlag(ItemFlags.Trap))
        {
          color = "red";
        }

        if (itemInfo.IsReceiverRelatedToActivePlayer)
        {
          __instance.description.text += $"Found <color={color}>{itemInfo.ItemDisplayName}</color>\n";
        }
        else
        {
          __instance.description.text +=
            $"Sent <color={color}>{itemInfo.ItemDisplayName}</color> to {itemInfo.Player.Alias}\n";
        }
      }
    }

    ItemsToSend.Clear();
    JustSentLocations = [];
  }

  /// <remarks>
  /// In Miracle Defibrillator, <see cref="CustomClearLocationPatch"/> will not fire.
  /// </remarks>
  /// <seealso cref="CustomClearLocationPatch"/>
  [HarmonyPatch(typeof(Level_Montage), nameof(Level_Montage.SetLevelAsPassed))]
  [HarmonyPostfix]
  private static void MiracleDefibrillatorClearLocationPatch(Level_Montage __instance)
  {
    bool hasScrambledCharacter = Plugin.Client.TrapManager.IsTrapActive(ScrambleCharactersTrapPatch.name);

    // We need to calculate the level's rank manually...
    int rank;
    if (!__instance.missedOnce && !__instance.game.GetPassedLevelWithoutCheckpoints())
    {
      // Perfect
      rank = Rank.BossPerfect;
    }
    else if (__instance.game.GetPassedLevelWithoutCheckpoints())
    {
      // Complete+
      rank = Rank.BossNoCheckpoints;
    }
    else
    {
      // Clear
      rank = Rank.BossClear;
    }
    IEnumerable<long> ids = SendLocations(Level.Montage, rank);

    if (!ItemsToSend.Any())
    {
      Plugin.Logger.LogWarning("Couldn't get items sent, possibly due to a network issue.");
      __instance.game.statusText.SetStatusText("Couldn't get items sent.", Color.red, 10f, useUnscaledTime: true);
    }
    else
    {
      long[] newLocations = ids.Where(id =>
          !Plugin.Client.Session.Locations.AllLocationsChecked.Contains(id) || JustSentLocations.Contains(id)
        )
        .ToArray();
      Plugin.Logger.LogDebug($"IDs: {string.Join(", ", ids)} (of which {string.Join(", ", newLocations)} are new)");

      if (!ids.Any())
      {
        __instance.game.statusText.SetStatusText("Didn't find anything.");
        return;
      }
      else if (newLocations.Length == 0)
      {
        __instance.game.statusText.SetStatusText("Didn't find anything new.");
        return;
      }

      IEnumerable<string> itemNames = from id in newLocations select ItemsToSend[id].ItemDisplayName;
      __instance.game.statusText.SetStatusText(
        $"Found {string.Join(", ", itemNames)}",
        duration: 10f,
        useUnscaledTime: true
      );
    }

    if (hasScrambledCharacter)
    {
      // Room 1, Row 2 - Cole Brew
      // At this point, any trap will be unapplied,
      //  so we do not need to worry about Scramble Characters re-scrambling Cole.
      Plugin.Logger.LogInfo("Setting Room 1, Row 2 to Cole");
      __instance.game.rows[1].ent.character.ChangeCharacter(Character.HoodieBoy);
    }
  }

  private static IEnumerable<long> SendLocations(Level level, Rank rank, bool bossLevelFailed = false)
  {
#if DEBUG
    // Discard Debug Menu traps regardless of result.
    Plugin.DebugMenu.TrapManager.ClearActiveTraps(false);
#endif
    if (bossLevelFailed)
    {
      Plugin.Client.TrapManager.ClearActiveTraps(false);
      return [];
    }

    IEnumerable<long> ids = GetStageLocationIDsToClear(level, rank);

    // Check if we fulfill the End Goal requirements
#pragma warning disable Harmony003
    if (Plugin.Client.Slot.endGoal == SlotData.EndGoal.HelpingHands && level == Level.HelpingHands && rank.passed)
#pragma warning restore Harmony003
    {
      Plugin.Logger.LogInfo("Setting goal achieved - Helping Hands");
      Plugin.Client.Session.SetGoalAchieved();
    }
    else if (Plugin.Client.Slot.endGoal != SlotData.EndGoal.HelpingHands)
    {
      bool clearedAll = true;
      Rank minimumRank;
      switch (Plugin.Client.Slot.endGoal)
      {
        case SlotData.EndGoal.PerfectAll:
          minimumRank = Rank.S;
          break;
        case SlotData.EndGoal.ARankAll:
          minimumRank = Rank.A;
          break;
        case SlotData.EndGoal.BRankAll:
          minimumRank = Rank.B;
          break;
        default:
          throw new ArgumentOutOfRangeException($"End Goal ({Plugin.Client.Slot.endGoal}) not valid value.");
      }
      foreach (Level otherLevel in Enum.GetValues(typeof(Level)))
      {
        Rank otherRank = Persistence.GetLevelRank(otherLevel);
        // If we aren't above the minimum rank, bail.
        if (minimumRank <= otherRank.ToNormal())
        {
          clearedAll = false;
          break;
        }
      }

      if (clearedAll)
      {
        Plugin.Logger.LogInfo("Setting goal achieved - Cleared all");
        Plugin.Client.Session.SetGoalAchieved();
      }
    }

    bool clearedNewLocation = ids.Any(id => !Plugin.Client.Session.Locations.AllLocationsChecked.Contains(id));
    if (clearedNewLocation)
    {
      JustSentLocations = ids.Where(id => !Plugin.Client.Session.Locations.AllLocationsChecked.Contains(id)).ToArray();
      Task.Run(() => Plugin.Client.Session.Locations.CompleteLocationChecksAsync(ids.ToArray()));
      Plugin.Client.TrapManager.ClearActiveTraps(false);
    }
    else
    {
      Plugin.Client.TrapManager.ClearActiveTraps(true);
    }

    return ids;
  }

  private static IEnumerator ScoutLocationChecks(long[] ids, int retries = 0)
  {
    Plugin.Logger.LogDebug($"Scouting location checks... (try {retries})");
    Task<Dictionary<long, ScoutedItemInfo>> scout = Task.Run(() =>
      Plugin.Client.Session.Locations.ScoutLocationsAsync(HintCreationPolicy.None, ids)
    );
    yield return new WaitUntil(() => scout.IsCompleted);
    Plugin.Logger.LogDebug("Completed scouting");

    if (!scout.IsCompletedSuccessfully)
    {
      if (retries >= 3)
      {
        Plugin.Logger.LogError($"Couldn't scout locations - on try #{retries}, retrying");
        Plugin.Instance.StartCoroutine(ScoutLocationChecks(ids, retries + 1));
        yield break;
      }

      Plugin.Logger.LogError("Couldn't scout locations");
      yield break;
    }

    ItemsToSend = scout.Result;
  }

  private static Level GetCurrentLevel()
  {
    if (!Enum.TryParse(scnGame.internalIdentifier, out Level level))
    {
      Plugin.Logger.LogError($"Couldn't find Level. Level identifier: {scnGame.internalIdentifier}");
      Plugin.Client.TrapManager.ClearActiveTraps(false);
      throw new ArgumentOutOfRangeException($"Couldn't find level {scnGame.internalIdentifier}");
    }

    return level;
  }

#pragma warning disable Harmony003
  private static IEnumerable<long> GetStageLocationIDsToClear(Level level, Rank rank)
  {
    Plugin.Logger.LogDebug("Getting locations to clear");

    IEnumerable<long> ids;
    if (scnGame.instance.currentLevel.dogMode && level == Level.Lesmis)
    {
      Plugin.Logger.LogInfo("Detected Rhythm Dogtor");
      // Playing 3-DOG - Rhythm Dogtor
      // ReSharper disable once NullableWarningSuppressionIsUsed
      BossStage rhythmDogtor = (Bindings.LevelToStage[level] as BossStage)!;

      List<long> idsToClear = [rhythmDogtor.ExtraLocations["dog_clear"]];
      if (rank.perfected)
      {
        idsToClear.Add(rhythmDogtor.ExtraLocations["dog_perfect"]);
      }

      ids = idsToClear.AsReadOnly();
    }
    else
    {
      ids = Bindings.LevelToStage[level].GetLocationsToClear(rank);
      Plugin.Logger.LogDebug($"Stage to clear: {level} with rank {rank} ({string.Join(", ", ids)})");
    }

    return ids;
  }
#pragma warning restore Harmony003
}
