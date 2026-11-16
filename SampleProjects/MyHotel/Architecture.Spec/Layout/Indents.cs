namespace MyHotel.Architecture.Spec.Layout;

/// <summary>A line's indentation as CODING-STANDARD-BACKEND.md 11c sets it: four spaces a level, never a tab
/// - one home for the facts that read it.</summary>
internal static class Indents
{
    public const int Width = 4;

    /// <summary>The spaces a line starts with.</summary>
    public static int Of(string line) => line.Length - line.TrimStart(' ').Length;

    /// <summary>The level a line starts at: its leading spaces in levels, a part of a level counting as
    /// one.</summary>
    public static int LevelOf(string line) => (Of(line) + Width - 1) / Width;
}
