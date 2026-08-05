namespace MyHotel.Core.Bookings;

/// <summary>
/// Where the last number issued is kept, so that numbering survives a restart. Null means nothing
/// has been issued yet and the seed stands in for it — the one moment the seed is consulted.
/// </summary>
public interface IBookingNumberStore
{
    Task<int?> LoadLastUsed();

    Task SaveLastUsed(int number);
}
