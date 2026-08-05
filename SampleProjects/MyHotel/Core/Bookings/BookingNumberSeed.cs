namespace MyHotel.Core.Bookings;

/// Where a hotel starts its numbering: the number it counts as already used, so the first booking
/// gets the one after. A type of its own rather than a bare int, so the number a specification
/// supplies says which number it is.
public record BookingNumberSeed(int LastUsed);
