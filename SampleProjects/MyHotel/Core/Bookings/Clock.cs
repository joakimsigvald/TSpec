namespace MyHotel.Core.Bookings;

internal static class Clock
{
    /// <summary>
    /// The hotel's date: the server's, in its own time zone.
    /// </summary>
    internal static DateOnly Today(this TimeProvider clock) =>
        DateOnly.FromDateTime(clock.GetLocalNow().DateTime);
}
