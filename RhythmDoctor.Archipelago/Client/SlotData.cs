namespace RhythmDoctor.Archipelago.Client;

using Newtonsoft.Json.Linq;

internal readonly struct SlotData
{
  internal enum EndGoal
  {
    HelpingHands = 0,
    BRankAll = 1,
    ARankAll = 2,
    PerfectAll = 3,
  }

  internal enum BossUnlockRequirement
  {
    BRankAll = 0,
    ARankAll = 1,
    Perfect = 2,
  }

  internal SlotData(Dictionary<string, object> slotData)
  {
    string msg = "Creating SlotData from";
    foreach ((string key, object value) in slotData)
    {
      msg += $" key: {key}, value: {value} (type {value.GetType().Name})";
    }
    Plugin.Logger.LogDebug(msg);

    endGoal = (EndGoal)(long)slotData["end_goal"];
    bossUnlockRequirement = (BossUnlockRequirement)(long)slotData["boss_unlock_requirement"];
    act1BossUnlockRequirement = (long)slotData["act_1_boss_unlock_requirement"];
    act2BossUnlockRequirement = (long)slotData["act_2_boss_unlock_requirement"];
    act3BossUnlockRequirement = (long)slotData["act_3_boss_unlock_requirement"];
    act4BossUnlockRequirement = (long)slotData["act_4_boss_unlock_requirement"];
    act5BossUnlockRequirement = (long)slotData["act_5_boss_unlock_requirement"];
    act6BossUnlockRequirement = (long)slotData["act_6_boss_unlock_requirement"];
    act7BossUnlockRequirement = (long)slotData["act_7_boss_unlock_requirement"];
    // https://stackoverflow.com/a/13565373
    // have to cast object to JArray first to use ToObject<T>
    // ReSharper disable once NullableWarningSuppressionIsUsed
    stickyTraps = ((JArray)slotData["sticky_traps"]).ToObject<List<string>>()!.ToArray();
    //stickyPowerups = ((JArray)slotData["sticky_powerups"]).ToObject<List<string>>()!.ToArray();
    deathLink = (long)slotData["death_link"] != 0;
    perfectRankLocations = (bool)slotData["perfect_rank_locations"];
    Plugin.Logger.LogDebug(
      $"Created SlotData - End Goal {endGoal}, Boss Unlock Requirement {bossUnlockRequirement}, [{act1BossUnlockRequirement}, {act2BossUnlockRequirement}, {act3BossUnlockRequirement}, {act4BossUnlockRequirement}, {act5BossUnlockRequirement}, {act6BossUnlockRequirement}, {act7BossUnlockRequirement}] Death Link {deathLink}"
    );
  }

  internal readonly EndGoal endGoal;
  internal readonly BossUnlockRequirement bossUnlockRequirement;
  internal readonly long act1BossUnlockRequirement;
  internal readonly long act2BossUnlockRequirement;
  internal readonly long act3BossUnlockRequirement;
  internal readonly long act4BossUnlockRequirement;
  internal readonly long act5BossUnlockRequirement;
  internal readonly long act6BossUnlockRequirement;
  internal readonly long act7BossUnlockRequirement;
  internal readonly string[] stickyTraps;
  internal readonly bool perfectRankLocations;

  //internal readonly string[] stickyPowerups;
  internal readonly bool deathLink;

  /// <summary>
  /// Gets the number of an act's levels that must be cleared with <see cref="bossUnlockRequirement"/>
  /// to unlock their respective boss song.
  /// </summary>
  /// <param name="act">Act to check.</param>
  /// <returns>Number of levels required to unlock the given act's boss song</returns>
  /// <exception cref="ArgumentOutOfRangeException">If given act is not an act.</exception>
  internal long GetBossSongLevelClearRequirement(Act act)
  {
    return act switch
    {
      Act.Act1 => act1BossUnlockRequirement,
      Act.Act2 => act2BossUnlockRequirement,
      Act.Act3 => act3BossUnlockRequirement,
      Act.Act4 => act4BossUnlockRequirement,
      Act.Act5 => act5BossUnlockRequirement,
      Act.Act6 => act6BossUnlockRequirement,
      Act.Act7 => act7BossUnlockRequirement,
      _ => throw new ArgumentOutOfRangeException(nameof(act), act, null),
    };
  }
}
