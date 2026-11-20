namespace MyHotel.Contract.Bookings;

/// <summary>
/// One method per endpoint, taking what the endpoint takes. Anything that would have been an early
/// return is thrown instead, so a method either returns the value the endpoint needs or does not
/// return at all.
/// </summary>
public interface IBookingService
{
    Task<IReadOnlyList<Booking>> Bookings();
    Task<Booking> BookingFor(int bookingNumber);
    Task<Booking> Book(BookingRequest request);
    Task<Booking> Change(int bookingNumber, BookingRequest request);
    Task Cancel(int bookingNumber);
    Task CheckIn(int bookingNumber);
    Task CheckOut(int bookingNumber);
    Task CancelBookings(string roomNumber);
}
