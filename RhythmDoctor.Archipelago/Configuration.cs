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
  private static ConfigEntry<Mode> _lastConnectedMode = null!;
  private static ConfigEntry<string> _lastConnectedUri = null!;
  private static ConfigEntry<string> _lastConnectedSlotName = null!;
  private static ConfigEntry<string> _lastConnectedPassword = null!;
  private static ConfigEntry<int> _autoReconnectMaxRetries = null!;
  private static ConfigEntry<int> _remoteTrapClearsTimeout = null!;
  private static ConfigEntry<int> _slotToUse = null!;

  // ReSharper restore NullableWarningSuppressionIsUsed

  internal static void Bind(ConfigFile config)
  {
    Plugin.Logger.LogInfo("Binding configuration");

    // TODO: Probably should put full description here.
    // Gameplay
    _deathLink = config.Bind(
      "Gameplay",
      "DeathLink",
      DeathLinkConfig.FollowSlot,
      "Whether to enable DeathLink when connected to an Archipelago slot."
        + "\nSee the respective Game Page in Archipelago for more info."
    );

    // zzyLastConnected
    // csharpier-ignore-start
    _lastConnectedMode = config.Bind(
      "zzyLastConnected",
      "Mode",
      Mode.Main,
      "Last connected slot's mode."
    );
    _lastConnectedUri = config.Bind(
      "zzyLastConnected",
      "Uri",
      "",
      "Last connected slot's URI."
    );
    _lastConnectedSlotName = config.Bind(
      "zzyLastConnected",
      "SlotName",
      "",
      "Last connected slot's name."
    );
    _lastConnectedPassword = config.Bind(
      "zzyLastConnected",
      "Password",
      "",
      "Last connected slot's password."
    );
    // csharpier-ignore-end

    // zzzDebugDoNotTouchUnlessAsked
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
      "How long to wait in milliseconds until getting remote trap clear status times out and defaults to 0."
    );

    _slotToUse = config.Bind("zzzDebugDoNotTouchUnlessAsked", "SlotToUse", 0, "Slot to use for Archipelago.");
  }

  internal static LoginInformation? PriorLoginInformation
  {
    get
    {
      // TODO: check for user putting in something invalid and breaking this :/
      if (
        // _lastConnectedMode.Value
        !_lastConnectedUri.Value.IsNullOrEmpty() && !_lastConnectedSlotName.Value.IsNullOrEmpty()
      )
        return new LoginInformation(
          _lastConnectedMode.Value,
          _lastConnectedUri.Value,
          _lastConnectedSlotName.Value,
          _lastConnectedPassword.Value
        );

      return null;
    }
    set
    {
      Plugin.Logger.LogDebug(
        $"[{nameof(Configuration)}] Updating prior login information: {value?.Mode}, {value?.Uri}, {value?.SlotName}, {value?.Password}"
      );
      _lastConnectedMode.Value = value!.Value.Mode;
      _lastConnectedUri.Value = value.Value.Uri;
      _lastConnectedSlotName.Value = value.Value.SlotName;
      _lastConnectedPassword.Value = value.Value.Password ?? "";
    }
  }

  internal static async Task<DeathLinkConfig> GetDeathLink()
  {
    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
    if (Plugin.StoryClient is null || Plugin.StoryClient.Session is null)
      return _deathLink.Value;

    return await Plugin.StoryClient.Session.DataStorage.GetRaceModeAsync()
      ? DeathLinkConfig.FollowSlot
      : _deathLink.Value;
  }

  /// <summary>
  /// Get the slot index to use for Archipelago.
  /// </summary>
  internal static int GetSlotToUse() => _slotToUse.Value;

  internal static int GetAutoReconnectMaxRetries() => _autoReconnectMaxRetries.Value;

  internal static int GetRemoteTrapClearsTimeout() => _remoteTrapClearsTimeout.Value;
}
