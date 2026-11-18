namespace MyHotel.Contract.Bookings.Exceptions;

public class InvalidGuestCount(int guestCount)
    : Exception($"A booking must be for at least one guest: {guestCount} is not.")
{
    public int GuestCount { get; } = guestCount;
}
