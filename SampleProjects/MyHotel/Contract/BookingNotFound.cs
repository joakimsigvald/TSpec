namespace MyHotel.Contract;

public class BookingNotFound(int bookingNumber)
    : Exception($"There is no booking {bookingNumber}.")
{
    public int BookingNumber { get; } = bookingNumber;
}
