using MyHotel.Contract.Bookings;
using static MyHotel.Contract.Bookings.BookingStatus;

namespace MyHotel.Core.Bookings;

/// <summary>
/// A booking as the store keeps it: what has happened to it, but not its status, since that changes
/// with the date while nothing is written. What can be done with it is what it allows, so the actions
/// it states and the ones it accepts cannot disagree.
/// </summary>
public record StoredBooking(
    int BookingNumber,
    string RoomNumber,
    string GuestName,
    int GuestCount,
    BookingPeriod Period,
    BookingStage Stage)
{
    internal Booking AsOf(DateOnly today) =>
        new(BookingNumber, new(RoomNumber, GuestName, GuestCount), Period, StateOn(today));

    internal bool Allows(BookingAction action, DateOnly today) =>
        action switch
        {
            BookingAction.Cancel => IsCancellableOn(today),
            BookingAction.CheckIn => StatusOn(today) == Open && today == Period.From,
            _ => StatusOn(today) == CheckedIn && today == Period.To,
        };

    internal bool IsCancellableOn(DateOnly today) =>
        StatusOn(today) == Open || (StatusOn(today) == NoShow && today <= Period.To);

    internal bool HoldsItsRoomOn(DateOnly today) => StatusOn(today) == CheckedIn || IsCancellableOn(today);
    internal StoredBooking After(BookingAction action) => this with { Stage = StageAfter(action) };

    internal StoredBooking ChangedTo(BookingRequest request) =>
        this with
        {
            RoomNumber = request.RoomNumber,
            GuestName = request.GuestName,
            GuestCount = request.GuestCount,
            Period = request.Period,
        };

    private BookingState StateOn(DateOnly today) =>
        new(StatusOn(today), [.. Enum.GetValues<BookingAction>().Where(action => Allows(action, today))]);

    /// <summary>
    /// The arrival and departure days still count: a booking turns no-show or past only the day after.
    /// </summary>
    private BookingStatus StatusOn(DateOnly today) =>
        Stage switch
        {
            BookingStage.Booked => today > Period.From ? NoShow : Open,
            BookingStage.CheckedIn => today > Period.To ? Past : CheckedIn,
            BookingStage.CheckedOut => CheckedOut,
            _ => Cancelled,
        };

    private static BookingStage StageAfter(BookingAction action) =>
        action switch
        {
            BookingAction.Cancel => BookingStage.Cancelled,
            BookingAction.CheckIn => BookingStage.CheckedIn,
            _ => BookingStage.CheckedOut,
        };
}
