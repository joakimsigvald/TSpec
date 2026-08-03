namespace MyHotel.Contract;

/// <summary>
/// One method per endpoint, taking what the endpoint takes. Anything that would have been an early
/// return is thrown instead, so a method either returns the value the endpoint needs or does not
/// return at all.
/// </summary>
public interface IBookingService
{
    Task<IReadOnlyList<Booking>> List();

    Task<Booking> Get(int id);

    Task<Booking> Book(BookingRequest request);

    Task Cancel(int id);
}
