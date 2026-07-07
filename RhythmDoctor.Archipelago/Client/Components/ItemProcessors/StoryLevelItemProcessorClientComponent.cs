namespace RhythmDoctor.Archipelago.Client.Components.ItemProcessors;

using System.Collections.ObjectModel;

internal class StoryLevelItemProcessorClientComponent : ItemProcessorClientComponent
{
  internal override async Task Enable(ArchipelagoSession session)
  {
    await base.Enable(session);
    // TODO: CACHE
    // throw new NotImplementedException();
  }

  internal override bool HandleItemInitial(ItemInfo itemInfo)
  {
    if (!Bindings.ItemIdToLevel.TryGetValue(itemInfo.ItemId, out Level level))
      return false; // Not a level.
    
    // Try to find if this level was cleared beforehand.
    if (!Bindings.LevelToStage.TryGetValue(level, out BaseStage stage))
    {
      Plugin.Logger.LogWarning($"[{nameof(StoryLevelItemProcessorClientComponent)}] Level {level} was found but couldn't find related Stage."
                               + " Ignoring any prior progress; setting rank to Rank.NotFinished");
      Persistence.SetLevelRank(level, Rank.NotFinished);
      return true;
    }
    
    if (level == Level.RhythmWeightlifter)
    {
      // TODO: set level rank for each of the 12 stages to cleared
      Plugin.Logger.LogInfo($"[{nameof(StoryLevelItemProcessorClientComponent)}] Handling Rhythm Weightlifter");
    
      // Rhythm Weightlifter is a special case in that it has 12 stages inside its level.
      // As the stages can only be played sequentially, and we don't have any specific Rank locations,
      //  we can take a shortcut and just set the last level unlocked to the number of
      //  Weightlifter locations we have cleared.
      int stagesCleared = _session.Locations.AllLocationsChecked.Count(locationId =>
        Bindings.RhythmWeightlifterStageToLocationID.Contains(locationId)
      );
    
      if (stagesCleared == 0)
      {
        // We haven't cleared any stages yet.
        Plugin.Logger.LogInfo($"[{nameof(StoryLevelItemProcessorClientComponent)}] Couldn't find any Rhythm Weightlifter locations");
      }
      else
      {
        Plugin.Logger.LogInfo($"[{nameof(StoryLevelItemProcessorClientComponent)}] Unlocking Rhythm Weightlifter stages up to stage {stagesCleared}");
        Persistence.SetRhythmWeightlifterLastLevelUnlocked(stagesCleared);
      }
    }
    else
    {
      // Normal level
      SetLevelBestRank(level, stage);
    }
    
    return true;
  }

  internal override bool HandleItem(ItemInfo itemInfo)
  {
    if (!Bindings.ItemIdToLevel.TryGetValue(itemInfo.ItemId, out Level level))
      return false; // Not a level.

    Persistence.SetLevelRank(level, Rank.NotFinished);
    return true;
  }

  // TODO: this.. probably shouldn't be in here?
  //       it's just here so UnlockItemPatch can use it for boss levels
  // TODO: could be optimized heavily, prevent iterating over ALL locations sent many many times.
  //       When Enabled, get the top rank for each level and cache it.
  //       Then retrieve it here.
  internal static void SetLevelBestRank(Level level, BaseStage stage)
  {
    Plugin.Logger.LogInfo($"[{nameof(StoryLevelItemProcessorClientComponent)}] Handling level");
    (Rank, long)[] stageLocationIds;

    switch (stage)
    {
      case RegularStage regularStage:
        stageLocationIds = [(Rank.S, regularStage.SRankLocation), (Rank.A, regularStage.ARankLocation),
          (Rank.B, regularStage.BRankLocation)];
        break;
      case BossStage bossStage:
        stageLocationIds = bossStage.CompletePlusLocation.HasValue
          ? [(Rank.BossPerfect, bossStage.PerfectLocation), (Rank.BossNoCheckpoints, bossStage.CompletePlusLocation.Value), (Rank.BossClear, bossStage.ClearLocation)]
          : [(Rank.BossPerfect, bossStage.PerfectLocation), (Rank.BossClear, bossStage.ClearLocation)];
        break;
      default:
        throw new InvalidOperationException("Can't get best rank for this type of Stage.");
    }

    ReadOnlyCollection<long> locations = Plugin.StoryClient.Session.Locations.AllLocationsChecked;
    
    // Locations are always sent in the order of B-A-S ranks, so if we iterate in reverse we always
    //  will catch the highest rank first.
    for (int sentLocationsIndex = locations.Count - 1; sentLocationsIndex >= 0; sentLocationsIndex--)
    {
      long locationId = locations[sentLocationsIndex];

      foreach ((Rank rank, long stageLocationId) in stageLocationIds)
      {
        if (stageLocationId == locationId)
          Persistence.SetLevelRank(level, rank);
      }
    }
    Persistence.SetLevelRank(level, Rank.NotFinished);
  }
}
