namespace MyHotel.Contract.Bookings.Exceptions;

public class NotDueForCheckIn(int bookingNumber)
    : Exception($"Booking {bookingNumber} is not open on its arrival day: it cannot be checked in.")
{
    public int BookingNumber { get; } = bookingNumber;
}
