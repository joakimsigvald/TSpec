namespace MyHotel.Contract.Bookings.Exceptions;

public class BookingNotCancellable(int bookingNumber)
    : Exception($"Booking {bookingNumber} cannot be cancelled: neither open nor a no-show yet to depart.")
{
    public int BookingNumber { get; } = bookingNumber;
}
