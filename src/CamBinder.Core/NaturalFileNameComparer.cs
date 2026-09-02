using System.Runtime.InteropServices;

namespace CamBinder.Core;

// Matches Windows Explorer's "Name" sort order (numbers compared numerically,
// e.g. "page2" before "page10"), rather than a plain ordinal string sort.
public sealed class NaturalFileNameComparer : IComparer<string?>
{
    public static readonly NaturalFileNameComparer Instance = new();

    public int Compare(string? x, string? y) => StrCmpLogicalW(x ?? string.Empty, y ?? string.Empty);

    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
    private static extern int StrCmpLogicalW(string psz1, string psz2);
}
