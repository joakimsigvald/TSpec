namespace MyHotel.Core.Spec.Bookings.BookingService;

/// <summary>
/// Every booking period is a valid stay, unless a specification says otherwise.
/// </summary>
public abstract class BookingServiceSpec<TResult> : Spec<Core.Bookings.BookingService, TResult>
{
    protected BookingServiceSpec() => Using(new BookingPeriod(Today, LaterDay));
}
