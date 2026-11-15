namespace MyHotel.Contract;

/// <summary>
/// What a refused request answers with: a code and nothing else, since the text shown for it is the
/// client's to choose (CODING-STANDARD.md, 7b).
/// </summary>
public sealed record ErrorBody(string Code);
