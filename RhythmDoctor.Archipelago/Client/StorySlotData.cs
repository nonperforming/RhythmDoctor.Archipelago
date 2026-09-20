namespace RhythmDoctor.Archipelago.Client;

using Newtonsoft.Json.Linq;

/// <summary>
/// Slot data for a Story world.
/// </summary>
internal readonly struct StorySlotData
{
  /// <summary>
  /// The end goal options.
  /// </summary>
  internal enum EndGoal
  {
    /// <summary>
    /// Clear all bosses and clear Helping Hands to goal.
    /// </summary>
    HelpingHands = 0,

    /// <summary>
    /// B rank (or higher) all levels to goal.
    /// </summary>
    BRankAll = 1,

    /// <summary>
    /// A rank (or higher) all levels to goal.
    /// </summary>
    ARankAll = 2,

    /// <summary>
    /// S rank/perfect all levels to goal.
    /// </summary>
    PerfectAll = 3,
  }

  /// <summary>
  /// The boss unlock rank requirement to unlock an act's boss.
  /// </summary>
  /// <seealso cref="act1BossUnlockRequirement"/>
  /// <seealso cref="act2BossUnlockRequirement"/>
  /// <seealso cref="act3BossUnlockRequirement"/>
  /// <seealso cref="act4BossUnlockRequirement"/>
  /// <seealso cref="act5BossUnlockRequirement"/>
  /// <seealso cref="act6BossUnlockRequirement"/>
  /// <seealso cref="act7BossUnlockRequirement"/>
  internal enum BossUnlockRequirement
  {
    /// <summary>
    /// B rank (or higher) the specified number of levels to unlock the act's boss.
    /// </summary>
    BRankAll = 0,

    /// <summary>
    /// A rank (or higher) the specified number of levels to unlock the act's boss.
    /// </summary>
    ARankAll = 1,

    /// <summary>
    /// S rank/perfect the specified number of levels to unlock the act's boss.
    /// </summary>
    Perfect = 2,
  }

  /// <summary>
  /// Create this slot's StorySlotData from Archipelago's SlotData dictionary.
  /// </summary>
  /// <param name="slotData">Archipelago's SlotData dictionary.</param>
  internal StorySlotData(Dictionary<string, object> slotData)
  {
    string msg = "Creating StorySlotData from";
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
    perfectRankLocations = (long)slotData["perfect_rank_locations"] != 0;
    Plugin.Logger.LogDebug(
      $"Created StorySlotData - End Goal {endGoal}, Boss Unlock Requirement {bossUnlockRequirement}, [{act1BossUnlockRequirement}, {act2BossUnlockRequirement}, {act3BossUnlockRequirement}, {act4BossUnlockRequirement}, {act5BossUnlockRequirement}, {act6BossUnlockRequirement}, {act7BossUnlockRequirement}] Death Link {deathLink}, Perfect Rank Locations {perfectRankLocations}"
    );
  }

  /// <summary>
  /// The end goal option selected for this slot.
  /// </summary>
  internal readonly EndGoal endGoal;

  /// <summary>
  /// The boss unlock rank requirement to unlock an act's boss.
  /// </summary>
  internal readonly BossUnlockRequirement bossUnlockRequirement;

  /// <summary>
  /// The number of levels in Act 1 that must be cleared with <see cref="bossUnlockRequirement"/>
  /// rank or higher to unlock 1-X - Battleworn Insomniac.
  /// </summary>
  internal readonly long act1BossUnlockRequirement;

  /// <summary>
  /// The number of levels in Act 2 that must be cleared with <see cref="bossUnlockRequirement"/>
  /// rank or higher to unlock 2-X - All The Times.
  /// </summary>
  internal readonly long act2BossUnlockRequirement;

  /// <summary>
  /// The number of levels in Act 3 that must be cleared with <see cref="bossUnlockRequirement"/>
  /// rank or higher to unlock 3-X - One Shift More and 3-DOG - Rhythm Dogtor.
  /// </summary>
  internal readonly long act3BossUnlockRequirement;

  /// <summary>
  /// The number of levels in Act 4 that must be cleared with <see cref="bossUnlockRequirement"/>
  /// rank or higher to unlock 1-XN - Super Battleworn Insomniac.
  /// </summary>
  internal readonly long act4BossUnlockRequirement;

  /// <summary>
  /// The number of levels in Act 5 that must be cleared with <see cref="bossUnlockRequirement"/>
  /// rank or higher to unlock 5-X - Dreams Don't Stop.
  /// </summary>
  internal readonly long act5BossUnlockRequirement;

  /// <summary>
  /// The number of levels in Act 6 that must be cleared with <see cref="bossUnlockRequirement"/>
  /// rank or higher to unlock 6-X - Boss Fight.
  /// </summary>
  internal readonly long act6BossUnlockRequirement;

  /// <summary>
  /// The number of levels in Act 7 that must be cleared with <see cref="bossUnlockRequirement"/>
  /// rank or higher to unlock 7-X - Miracle Defibrillator and 7-X2 - Miracle Defibrillator (Cole's Song).
  /// </summary>
  internal readonly long act7BossUnlockRequirement;

  /// <summary>
  /// The names of the traps that should always be applied whenever possible.
  /// </summary>
  /// <remarks>
  /// This needs to be converted to a trao's UID: see <see cref="Bindings"/>.
  /// </remarks>
  // TODO: When Sticky Traps are implemented, fix the cref to link to the specific field.
  internal readonly string[] stickyTraps;

  /// <summary>
  /// Dictates if perfect rank locations are included in the world or not.
  /// </summary>
  internal readonly bool perfectRankLocations;

  /// <summary>
  /// The names of the traps that should always be applied whenever possible.
  /// </summary>
  /// <remarks>
  /// This needs to be converted to a powerup's UID: see <see cref="Bindings"/>.
  /// </remarks>
  // TODO: When Sticky Traps are implemented, fix the cref to link to the specific field.
  //internal readonly string[] stickyPowerups;

  /// <summary>
  /// Dictates if DeathLink is on for this slot.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Only follow this option if the user's selected DeathLink option in
  /// <see cref="Configuration._deathLink"/> is <see cref="Configuration.DeathLinkConfig.FollowSlot"/>.
  /// </para>
  /// <para>
  /// However, if race mode is on, the user's selected DeathLink option
  /// should be ignored and DeathLink should be forced to the slot's option.
  /// </para>
  /// </remarks>
  /// <seealso cref="Configuration._deathLink"/>
  /// <seealso cref="Configuration.DeathLinkConfig"/>
  internal readonly bool deathLink;

  /// <summary>
  /// Gets the number of an act's levels that must be cleared with <see cref="bossUnlockRequirement"/>
  /// to unlock their respective boss song.
  /// </summary>
  /// <param name="act">Act to check.</param>
  /// <returns>Number of levels required to unlock the given act's boss song</returns>
  /// <exception cref="ArgumentException">If given act is None.</exception>
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
      Act.None => throw new ArgumentException("Cannot get boss levels to clear for None act", nameof(act)),
      _ => throw new ArgumentOutOfRangeException(nameof(act), act, null),
    };
  }
}
