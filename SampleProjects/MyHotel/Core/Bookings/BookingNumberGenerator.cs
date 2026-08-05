namespace MyHotel.Core.Bookings;

/// <summary>
/// Issues booking numbers in order, recording each as the last used. Nothing here consults the
/// bookings: a number is spent when it is issued, so cancelling a booking leaves it spent.
/// </summary>
public class BookingNumberGenerator(IBookingNumberStore store, BookingNumberSeed seed)
    : IBookingNumberGenerator
{
    public async Task<int> Next()
    {
        var number = (await store.LoadLastUsed() ?? seed.LastUsed) + 1;
        await store.SaveLastUsed(number);
        return number;
    }
}
