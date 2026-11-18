using static MyHotel.Contract.Bookings.BookingStatus;

namespace MyHotel.Core.Spec.Bookings.BookingService;

/// <summary>
/// A booking's status is worked out from its dates and today's date, so what the store holds only
/// says whether it was cancelled.
/// </summary>
public abstract class WhenGet : Spec<Core.Bookings.BookingService, Booking>
{
    protected WhenGet() =>
        Using(TodayIs(new(2026, 8, 10)))
            .When(_ => _.BookingFor(A<StoredBooking>().BookingNumber));

    /// <summary>
    /// The guest departs today, so the booking is still current.
    /// </summary>
    public class GivenTheBookingDepartsToday : WhenGet
    {
        public GivenTheBookingDepartsToday() =>
            Given<IBookingStore>().That(_ => _.Load())
                .Returns(() => [The<StoredBooking>() with { To = new(2026, 8, 10), Cancelled = false }]);

        [Fact]
        public void ThenReturnTheBooking() =>
            Result.BookingNumber.Is(The<StoredBooking>().BookingNumber);

        [Fact] public void ThenSayItIsOpen() => Result.Status.Is(Open);
    }

    public class GivenTheBookingDepartedYesterday : WhenGet
    {
        public GivenTheBookingDepartedYesterday() =>
            Given<IBookingStore>().That(_ => _.Load())
                .Returns(() => [The<StoredBooking>() with { To = new(2026, 8, 9), Cancelled = false }]);

        [Fact] public void ThenSayItIsPast() => Result.Status.Is(Past);
    }

    public class GivenTheBookingWasCancelled : WhenGet
    {
        public GivenTheBookingWasCancelled() =>
            Given<IBookingStore>().That(_ => _.Load())
                .Returns(() => [The<StoredBooking>() with { To = new(2026, 8, 12), Cancelled = true }]);

        [Fact] public void ThenSayItIsCancelled() => Result.Status.Is(Cancelled);
    }

    public class GivenNoSuchBooking : WhenGet
    {
        public GivenNoSuchBooking() =>
            Given<IBookingStore>().That(_ => _.Load()).Returns(Zero<StoredBooking>);

        [Fact]
        public void ThenThrowBookingNotFound() =>
            Then().Throws<BookingNotFound>()
                .that.Message.Does().Contain($"{The<StoredBooking>().BookingNumber}");
    }
}
