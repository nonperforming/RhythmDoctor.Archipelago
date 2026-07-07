namespace RhythmDoctor.Archipelago.Client;

/// <summary>
/// Archipelago client state.
/// </summary>
internal enum ClientState
{
  /// <summary>
  /// The client is not ready for any work yet,
  /// and should not be used yet.
  /// </summary>
  NotReady,

  /// <summary>
  /// The client is creating a session and should not be used yet.
  /// </summary>
  CreatingSession,

  /// <summary>
  /// The client has created a session and is waiting for login.
  /// It should not be used yet.
  /// </summary>
  CreatedSession,

  /// <summary>
  /// The client is attempting to log into the Archipelago server.
  /// It should not be used yet.
  /// </summary>
  LoggingIn,

  /// <summary>
  /// The client is logged in.
  /// It should not be used yet.
  /// </summary>
  LoggedIn,

  /// <summary>
  /// The client is processing previously collected items from the Archipelago server.
  /// It should not be used yet.
  /// </summary>
  ReceivingPriorItems,

  /// <summary>
  /// The client has processed all prior items; it is ready for use, and able to receive new items.
  /// </summary>
  Ready,

  /// <summary>
  /// The client is disposed and should not be used.
  /// </summary>
  Disposed,

  /// <summary>
  /// The client failed to initialize or login and should not be used.
  /// </summary>
  Failed,
}
