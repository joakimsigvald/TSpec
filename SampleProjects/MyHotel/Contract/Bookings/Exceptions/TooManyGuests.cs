namespace MyHotel.Contract.Bookings.Exceptions;

public class TooManyGuests(string roomNumber, int guestCount)
    : Exception($"Room {roomNumber} has too few beds for {guestCount} guests.")
{
    public string RoomNumber { get; } = roomNumber;
    public int GuestCount { get; } = guestCount;
}
