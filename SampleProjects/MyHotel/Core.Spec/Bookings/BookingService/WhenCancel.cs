using static Moq.Times;

namespace MyHotel.Core.Spec.Bookings.BookingService;

public abstract class WhenCancel : Spec<Core.Bookings.BookingService>
{
    protected WhenCancel() => When(_ => _.Cancel(A<Booking>().Id));

    public class GivenTheBookingExists : WhenCancel
    {
        public GivenTheBookingExists()
            => Given<IBookingStore>().That(_ => _.Load()).Returns(One<Booking>);

        [Fact]
        public void ThenStoreWhatIsLeft() => Then<IBookingStore>(_ => _.Save(Zero<Booking>()));
    }

    public class GivenNoSuchBooking : WhenCancel
    {
        public GivenNoSuchBooking()
            => Given<IBookingStore>().That(_ => _.Load()).Returns(Zero<Booking>);

        [Fact]
        public void ThenThrowBookingNotFound()
            => Then().Throws<BookingNotFound>()
                .that.Message.Does().Contain($"{The<Booking>().Id}");

        [Fact] public void ThenStoreNothing() => Then<IBookingStore>(nameof(IBookingStore.Save), Never);
    }
}
