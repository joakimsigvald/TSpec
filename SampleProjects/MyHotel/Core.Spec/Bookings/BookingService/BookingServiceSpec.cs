namespace MyHotel.Core.Spec.Bookings.BookingService;

/// <summary>
/// Every booking period is a valid stay, and no booking is paid, unless a specification says otherwise.
/// </summary>
public abstract class BookingServiceSpec<TResult> : Spec<Core.Bookings.BookingService, TResult>
{
    protected BookingServiceSpec() =>
        Using(TodayIs(Today))
        .And(new BookingPeriod(Today, LaterDay))
        .And((StoredBooking booking) => booking with { Paid = false });
}
