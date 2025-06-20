namespace RhythmDoctor.Archipelago.Interfaces;

internal interface ITrap
{
  internal string Name { get; }
  internal Type[] IncompatibleWith { get; }
}

// ReSharper disable once InconsistentNaming
internal static class ITrapExtensions
{
  internal static bool IsIncompatibleWith(this ITrap self, ITrap other) => IsIncompatibleWith(self, other.GetType());

  internal static bool IsIncompatibleWith(this ITrap self, Type other) => self.IncompatibleWith.Contains(other);

  internal static bool IsIncompatibleWith(this ITrap self, IEnumerable<ITrap> otherTraps) =>
    otherTraps.All(otherTrap => !self.IsIncompatibleWith(otherTrap));
}
