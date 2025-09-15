namespace RhythmDoctor.Archipelago.Client;

internal struct SlotData
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
    Half = 0,
    BRankAll = 1,
    ARankAll = 2,
    Perfect = 3,
  }

  internal SlotData(Dictionary<string, object> slotData)
  {
    string msg = "Creating SlotData from";
    foreach ((string key, object value) in slotData)
    {
      msg += $"  key: {key}, value: {value} (type {value.GetType().Name})";
    }
    Plugin.Logger.LogDebug(msg);

    endGoal = (EndGoal)(long)slotData["end_goal"];
    bossUnlockRequirement = (BossUnlockRequirement)(long)slotData["boss_unlock_requirement"];
    deathLink = (long)slotData["death_link"] != 0;
    Plugin.Logger.LogDebug($"Created SlotData - {endGoal}, {bossUnlockRequirement},  {deathLink}");
  }

  internal readonly EndGoal endGoal;
  internal readonly BossUnlockRequirement bossUnlockRequirement;
  internal readonly bool deathLink;
}
