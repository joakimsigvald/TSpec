using MyHotel.Contract.Bookings;

namespace MyHotel.Core.Bookings;

/// <summary>
/// A booking as the store keeps it: whether it was cancelled, but not whether it is past, since
/// that changes with the date while nothing is written.
/// </summary>
public record StoredBooking(
    int BookingNumber,
    string RoomNumber,
    string GuestName,
    int GuestCount,
    BookingPeriod Period,
    bool Cancelled)
{
    internal Booking AsOf(DateOnly today) =>
        new(BookingNumber, RoomNumber, GuestName, GuestCount, Period, StatusOn(today));

    internal bool IsOpenOn(DateOnly today) => StatusOn(today) == BookingStatus.Open;
    internal StoredBooking AsCancelled() => this with { Cancelled = true };

    /// <summary>
    /// The guest's departure day still counts: a booking is past only from the day after.
    /// </summary>
    private BookingStatus StatusOn(DateOnly today) =>
        this switch
        {
            { Cancelled: true } => BookingStatus.Cancelled,
            _ when Period.To < today => BookingStatus.Past,
            _ => BookingStatus.Open,
        };
}
