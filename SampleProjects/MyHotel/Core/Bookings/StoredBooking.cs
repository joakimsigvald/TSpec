using MyHotel.Contract.Bookings;
using static MyHotel.Contract.Bookings.BookingStatus;

namespace MyHotel.Core.Bookings;

/// <summary>
/// A booking as the store keeps it: what has happened to it, but not its status, since that changes
/// with the date while nothing is written, nor its price, which follows its nights. What can be done
/// with it is what it allows, so the actions it states and the ones it accepts cannot disagree.
/// </summary>
public record StoredBooking(
    int BookingNumber,
    BookingInfo Info,
    BookingPeriod Period,
    int PricePerNight,
    BookingStage Stage,
    bool Paid)
{
    internal Booking AsOf(DateOnly today) => new(BookingNumber, Info, Period, StateOn(today), Payment);

    internal bool Allows(BookingAction action, DateOnly today) =>
        action switch
        {
            BookingAction.Cancel => IsCancellableOn(today),
            BookingAction.CheckIn => StatusOn(today) == Open && today == Period.From,
            BookingAction.CheckOut => StatusOn(today) == CheckedIn && today == Period.To,
            _ => !Paid && StatusOn(today) is Open or CheckedIn,
        };

    internal bool IsCancellableOn(DateOnly today) =>
        StatusOn(today) == Open || (StatusOn(today) == NoShow && today <= Period.To);

    internal bool HoldsItsRoomOn(DateOnly today) => StatusOn(today) == CheckedIn || IsCancellableOn(today);

    internal StoredBooking After(BookingAction action) =>
        action switch
        {
            BookingAction.Cancel => this with { Stage = BookingStage.Cancelled },
            BookingAction.CheckIn => this with { Stage = BookingStage.CheckedIn },
            BookingAction.CheckOut => this with { Stage = BookingStage.CheckedOut },
            _ => this with { Paid = true },
        };

    internal StoredBooking ChangedTo(BookingRequest request, int pricePerNight) =>
        this with { Info = request.Info, Period = request.Period, PricePerNight = pricePerNight };

    private BookingPayment Payment => new(Nights * PricePerNight, Paid);
    private int Nights => Period.To.DayNumber - Period.From.DayNumber;

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
}
