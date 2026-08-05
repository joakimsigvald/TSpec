namespace MyHotel.Core.Bookings;

/// <summary>
/// Where a booking's number comes from. Behind an interface because the number a booking gets is
/// not a fact about the bookings already made — a hotel starts its numbering where it likes, and a
/// specification of what else <c>Book</c> does should not have to know which number that is.
/// </summary>
public interface IBookingNumberGenerator
{
    /// The next number to issue. Every call yields a new one; none is ever issued twice.
    Task<int> Next();
}
