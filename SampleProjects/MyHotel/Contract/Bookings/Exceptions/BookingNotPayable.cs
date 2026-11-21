namespace MyHotel.Contract.Bookings.Exceptions;

public class BookingNotPayable(int bookingNumber)
    : Exception($"Booking {bookingNumber} cannot be paid: neither open nor checked in, or paid already.")
{
    public int BookingNumber { get; } = bookingNumber;
}
