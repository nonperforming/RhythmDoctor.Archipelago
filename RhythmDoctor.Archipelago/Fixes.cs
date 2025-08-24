// Consider using PolySharp if too many hacks are required to build

#region CS0518
// Predefined type 'System.Runtime.CompilerServices.IsExternalInit' is not defined or imported
// Exclusively fails on GitHub Actions
//
// From https://stackoverflow.com/a/62656145
using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
  [EditorBrowsable(EditorBrowsableState.Never)]
  internal class IsExternalInit { }
}
#endregion
