namespace MyHotel.Core.Spec.Bookings.BookingService;

/// <summary>
/// The booking is priced anew: its two nights at the price per night it keeps, or takes from the room it
/// moves to.
/// </summary>
public abstract class WhenChangeToTwoNights : BookingServiceSpec<Booking>
{
    protected WhenChangeToTwoNights() =>
        Using(TodayIs(Today))
            .And(new BookingInfo(A<Room>().RoomNumber, A<string>(), 1))
            .When(_ => _.Change(
                A<StoredBooking>().BookingNumber,
                A<BookingRequest>() with { Period = new(Today, Today.AddDays(2)) }));

    public class GivenItStaysInItsRoom : WhenChangeToTwoNights
    {
        public GivenItStaysInItsRoom() =>
            Given<IRoomStore>().That(_ => _.Load()).Returns(() => [The<Room>()])
                .And<IBookingStore>().That(_ => _.Load()).Returns(() => [The<StoredBooking>()]);

        [Fact]
        public void ThenKeepItsPricePerNight() =>
            Result.Payment.Price.Is(2 * The<StoredBooking>().PricePerNight);
    }

    public abstract class GivenItIsInAnotherRoom : WhenChangeToTwoNights
    {
        protected GivenItIsInAnotherRoom() =>
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The<StoredBooking>() with
            {
                Info = The<BookingInfo>() with { RoomNumber = ASecond<Room>().RoomNumber },
            }]);

        public class OfTheSameCategory : GivenItIsInAnotherRoom
        {
            public OfTheSameCategory() =>
                Given<IRoomStore>().That(_ => _.Load()).Returns(() => [
                    The<Room>(),
                    ASecond<Room>() with { RoomCategory = The<Room>().RoomCategory }]);

            [Fact]
            public void ThenKeepItsPricePerNight() =>
                Result.Payment.Price.Is(2 * The<StoredBooking>().PricePerNight);
        }

        public class OfAnotherCategory : GivenItIsInAnotherRoom
        {
            public OfAnotherCategory() =>
                Given<IRoomStore>().That(_ => _.Load()).Returns(() => [The<Room>(), ASecond<Room>()]);

            [Fact]
            public void ThenTakeThePricePerNightOfTheNewRoom() =>
                Result.Payment.Price.Is(2 * The<Room>().PricePerNight);
        }

        /// <summary>
        /// A room no longer there counts as another category.
        /// </summary>
        public class ThatIsGone : GivenItIsInAnotherRoom
        {
            public ThatIsGone() => Given<IRoomStore>().That(_ => _.Load()).Returns(() => [The<Room>()]);

            [Fact]
            public void ThenTakeThePricePerNightOfTheNewRoom() =>
                Result.Payment.Price.Is(2 * The<Room>().PricePerNight);
        }
    }
}
