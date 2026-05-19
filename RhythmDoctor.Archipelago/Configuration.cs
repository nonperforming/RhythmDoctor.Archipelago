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
  private static ConfigEntry<int> _autoReconnectMaxRetries = null!;
  private static ConfigEntry<int> _remoteTrapClearsTimeout = null!;
  private static ConfigEntry<int> _slotToUse = null!;

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

    _autoReconnectMaxRetries = config.Bind(
      "zzzDebugDoNotTouchUnlessAsked", // show at end
      "AutoReconnectMaxRetries",
      3,
      "How many times to attempt to reconnect to an Archipelago server "
        + "before the client is considered unsalvagable and the Client is abandoned."
    );

    _remoteTrapClearsTimeout = config.Bind(
      "zzzDebugDoNotTouchUnlessAsked",
      "RemoteTrapClearsTimeout",
      3000,
      "How long to wait in milliseconds until getting remote trap clear status times out "
        + "and defaults to 0/last known good value."
    );

    _slotToUse = config.Bind("zzzDebugDoNotTouchUnlessAsked", "SlotToUse", 0, "Slot to use for Archipelago.");
  }

  internal static async Task<DeathLinkConfig> GetDeathLink()
  {
    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
    if (Plugin.Client is null || Plugin.Client.Session is null)
      return _deathLink.Value;

    return await Plugin.Client.Session.DataStorage.GetRaceModeAsync() ? DeathLinkConfig.FollowSlot : _deathLink.Value;
  }

  /// <summary>
  /// Get the slot index to use for Archipelago.
  /// </summary>
  internal static int GetSlotToUse() => _slotToUse.Value;

  internal static int GetAutoReconnectMaxRetries() => _autoReconnectMaxRetries.Value;

  internal static int GetRemoteTrapClearsTimeout() => _remoteTrapClearsTimeout.Value;
}
