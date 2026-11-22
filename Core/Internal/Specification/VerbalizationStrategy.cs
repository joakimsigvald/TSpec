namespace TSpec.Internal.Specification;

/// <summary>
/// How an assertion is said: as written, in the third person ("contains"), or as an assertion stated of each
/// item of its actual ("For each of arr:", the item's assertion below it).
/// </summary>
internal enum VerbalizationStrategy { None, PresentSingularS, ForEach }
