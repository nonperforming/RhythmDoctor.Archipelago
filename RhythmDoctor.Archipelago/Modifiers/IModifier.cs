namespace RhythmDoctor.Archipelago.Modifiers;

/// <summary>
/// A level modifier.
/// </summary>
/// <remarks>
/// All modifiers should be registered to the <see cref="ModifierRegistry"/>
/// using <see cref="ModifierRegistry.Register(IModifier)"/>
/// or <see cref="ModifierRegistry.Register(IModifier[])"/>
/// </remarks>
internal interface IModifier
{
  /// <summary>
  /// The name of the modifier.
  /// </summary>
  [Pure]
  string Name { get; }
  
  /// <summary>
  /// Compatibility attributes of the modifier.
  /// </summary>
  [Pure]
  ModifierCompatibility Compatibility { get; }
  
  /// <summary>
  /// Capabilities of the modifier.
  /// This should include anything the modifier might change.
  /// </summary>
  [Pure]
  ModifierCapability Capability { get; }

  /// <summary>
  /// Run when the modifier is instantiated.
  /// For the lifetime of the game this will only be called once at most.
  /// </summary>
  internal void Initialize();

  /// <summary>
  /// Run when a compatible level is selected (still in menu).
  /// </summary>
  /// <param name="strength">Modifier strength being applied to the level.</param>
  internal void Preview(int strength);

  /// <summary>
  /// Run when a compatible level is unselected.
  /// </summary>
  internal void PreviewEnd();

  /// <summary>
  /// Run just before transitioning to a compatible level.
  /// </summary>
  /// <param name="strength">Modifier strength being applied to the level.</param>
  internal void Active(int strength);
  
  /// <summary>
  /// Run just before exiting a compatible level.
  /// </summary>
  internal void ActiveEnd();
}
