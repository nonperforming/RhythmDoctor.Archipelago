namespace RhythmDoctor.Archipelago.Interfaces;

/// <summary>
/// Represents a trap or powerup.
/// </summary>
/// <seealso cref="TrapManager"/>
/// <seealso cref="TrapManagerPatch"/>
internal interface ITrap
{
  /// <summary>
  /// The human-facing name of this Trap.
  /// </summary>
  [Pure]
  internal string Name { get; }

  /// <summary>
  /// Other trap Types that are incompatible with this trap.
  /// </summary>
  [Pure]
  internal IEnumerable<Type> IncompatibleWithTraps { get; }

  /// <summary>
  /// Levels that are incompatible with this trap.
  /// </summary>
  [Pure]
  internal IEnumerable<Level> IncompatibleWithLevels =>
    LevelExtensions.AllBonusLevels.Concat(LevelExtensions.AllIntermissionLevels);

  /// <summary>
  /// Put additional compatibility checks not covered by <see cref="IncompatibleWithTraps"/> or
  /// <see cref="IncompatibleWithLevels"/> here.
  /// </summary>
  /// <returns><c>true</c> if compatible, otherwise <c>false</c></returns>
  [Pure]
  internal bool Compatible(Level level)
  {
    return true;
  }

  /// <summary>
  /// Run when this trap is added to the queue.
  /// </summary>
  /// <remarks>
  /// This method will be run regardless of whether it is compatible or not.
  /// </remarks>
  /// <seealso cref="TrapManager"/>
  internal void InQueue() { }

  /// <summary>
  /// Run just before a level is highlighted when this trap is applicable.
  /// </summary>
  /// <remarks>
  /// You should also define <see cref="PreviewLevelEnd"/> when implementing this.
  /// </remarks>
  internal void PreviewLevel() { }

  /// <summary>
  /// Run when a level is un-highlighted when previously this trap was applicable.
  /// </summary>
  /// <remarks>
  /// You should also define <see cref="PreviewLevel"/> when implementing this.
  /// This method may not be run when preview traps are promoted to active traps.
  /// </remarks>
  internal void PreviewLevelEnd() { }

  /// <summary>
  /// Run just before transitioning to a level when this trap is applicable.
  /// </summary>
  /// <remarks>
  /// You should also define <see cref="ActiveEnd"/> when implementing this.
  /// </remarks>
  internal void Active();

  /// <summary>
  /// Run just before exiting a level where this trap was Active.
  /// </summary>
  /// <remarks>
  /// You should also define <see cref="ActiveEnd"/> when implementing this.
  /// </remarks>
  internal void ActiveEnd();
}

// ReSharper disable once InconsistentNaming
/// <summary>
/// Extension methods for <see cref="ITrap"/> to simplify compatibility checks.
/// </summary>
internal static class ITrapExtensions
{
  /// <summary>
  /// Check if this trap is compatible with another trap on a specific level.
  /// </summary>
  /// <param name="self">This trap.</param>
  /// <param name="other">The other trap to test against.</param>
  /// <param name="level">The level to test against.</param>
  /// <returns><c>false</c> if compatible with the other trap, otherwise <c>true</c></returns>
  /// <remarks>
  /// This trap's <see cref="ITrap.Compatible"/> is called, but the other traps' <see cref="ITrap.Compatible"/> is not.
  /// </remarks>
  internal static bool IsIncompatibleWith(this ITrap self, ITrap other, Level level) =>
    IsIncompatibleWith(self, other.GetType(), level);

  /// <summary>
  /// Check if this trap is compatible with another trap type on a specific level.
  /// </summary>
  /// <param name="self">This trap.</param>
  /// <param name="other">The other trap type to test against.</param>
  /// <param name="level">The level to test against.</param>
  /// <returns><c>false</c> if compatible with the other trap type, otherwise <c>true</c></returns>
  /// <remarks>
  /// This trap's <see cref="ITrap.Compatible"/> is called, but the other traps' <see cref="ITrap.Compatible"/> is not.
  /// </remarks>
  internal static bool IsIncompatibleWith(this ITrap self, Type other, Level level) =>
    self.IncompatibleWithTraps.Contains(other)
    || self.IncompatibleWithLevels.Contains(level)
    || !self.Compatible(level);

  /// <summary>
  /// Check if this trap is compatible with a specific level.
  /// </summary>
  /// <param name="self">This trap.</param>
  /// <param name="level">The level to test against.</param>
  /// <returns><c>false</c> if compatible with the level, otherwise <c>true</c></returns>
  internal static bool IsIncompatibleWith(this ITrap self, Level level) =>
    self.IncompatibleWithLevels.Contains(level) || !self.Compatible(level);

  /// <summary>
  /// Check if this trap is compatible with a set of traps on a specific level.
  /// </summary>
  /// <param name="self">This trap.</param>
  /// <param name="others">The other traps to test against.</param>
  /// <param name="level">The level to test against.</param>
  /// <returns><c>false</c> if this trap is compatible with all the other traps, otherwise <c>true</c></returns>
  /// <remarks>
  /// This trap's <see cref="ITrap.Compatible"/> is called, but the other traps' <see cref="ITrap.Compatible"/> is not.
  /// </remarks>
  internal static bool IsIncompatibleWith(this ITrap self, IEnumerable<ITrap> others, Level level) =>
    others.Any(otherTrap => self.IsIncompatibleWith(otherTrap, level)) || self.IsIncompatibleWith(level); // If there are no other traps, the first condition will return false.

  /// <summary>
  /// Check if this trap is compatible with a set of traps on a specific level.
  /// </summary>
  /// <param name="self">This trap.</param>
  /// <param name="others">The other traps to test against.</param>
  /// <param name="level">The level to test against.</param>
  /// <returns><c>false</c> if this trap is compatible with all the other traps, otherwise <c>true</c></returns>
  /// <remarks>
  /// This trap's <see cref="ITrap.Compatible"/> is called, but the other traps' <see cref="ITrap.Compatible"/> is not.
  /// </remarks>
  internal static bool IsIncompatibleWith(this ITrap self, IEnumerable<(int index, ITrap trap)> others, Level level) =>
    self.IsIncompatibleWith(others.Select(tuple => tuple.trap), level);
}
