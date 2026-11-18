namespace MyHotel.Contract.Bookings.Exceptions;

public class BookingNotOpen(int bookingNumber)
    : Exception($"Booking {bookingNumber} is past or cancelled, so it cannot be cancelled.")
{
    public int BookingNumber { get; } = bookingNumber;
}
