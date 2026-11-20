namespace MyHotel.Contract.Bookings.Exceptions;

public class NotDueForCheckOut(int bookingNumber)
    : Exception($"Booking {bookingNumber} is not checked in on its departure day: it cannot be checked out.")
{
    public int BookingNumber { get; } = bookingNumber;
}
