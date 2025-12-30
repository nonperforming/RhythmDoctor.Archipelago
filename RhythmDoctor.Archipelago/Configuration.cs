namespace RhythmDoctor.Archipelago;

internal static class Configuration
{
  internal enum DeathLinkConfig
  {
    FollowSlot,
    On,
    Off,
  }

  // ReSharper disable NullableWarningSuppressionIsUsed
  private static ConfigEntry<DeathLinkConfig> _deathLink = null!;

  // ReSharper restore NullableWarningSuppressionIsUsed

  internal static void Bind(ConfigFile config)
  {
    Plugin.Logger.LogInfo("Binding configuration");

    // TODO: Probably should put full description here.
    _deathLink = config.Bind(
      "Gameplay",
      "DeathLink",
      DeathLinkConfig.FollowSlot,
      "Whether to enable DeathLink when connected to an Archipelago slot."
        + "\nSee the respective Game Page in Archipelago for more info."
    );
  }

  internal static DeathLinkConfig GetDeathLink()
  {
    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
    if (Plugin.Client is null || Plugin.Client.Session is null)
      return _deathLink.Value;

    return Plugin.Client.Session.DataStorage.GetRaceMode() ? DeathLinkConfig.FollowSlot : _deathLink.Value;
  }
}
