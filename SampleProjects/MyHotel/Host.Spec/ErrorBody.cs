namespace MyHotel.Spec;

/// <summary>
/// The body a refused request carries. Asserting on it is what tells a handled refusal from a route
/// that is simply not there — an unmatched path answers 404 with nothing in it.
/// </summary>
public record ErrorBody(string Error);
