namespace MyHotel.Core.Spec.Rooms.RoomService;

public abstract class WhenListRooms : Spec<Core.Rooms.RoomService, IReadOnlyList<RoomOverview>>
{
    protected WhenListRooms() => When(_ => _.Rooms());

    public class GivenNoRooms : WhenListRooms
    {
        public GivenNoRooms() => Given<IRoomStore>().That(_ => _.Load()).Returns(Zero<Room>);

        [Fact] public void ThenReturnNoRooms() => Result.Is().Empty();
    }

    /// <summary>
    /// Both rooms are held, but only the first has bookings to cancel, as when the second one's guest
    /// is checked in.
    /// </summary>
    public class GivenTwoRooms : WhenListRooms
    {
        public GivenTwoRooms() =>
            Given<IRoomStore>().That(_ => _.Load()).Returns(Two<Room>)
                .And<IRoomBookings>().That(_ => _.HeldRooms())
                .Returns(() => [TheFirst<Room>().RoomNumber, TheSecond<Room>().RoomNumber])
                .AndThat(_ => _.RoomsWithCancellableBookings()).Returns(() => [TheFirst<Room>().RoomNumber]);

        [Fact]
        public void ThenReturnThemInTheOrderTheyAreStored() =>
            Result.Is().EqualTo([
                new RoomOverview(
                    TheFirst<Room>().RoomNumber,
                    TheFirst<Room>().BedCount,
                    TheFirst<Room>().RoomCategory,
                    TheFirst<Room>().PricePerNight,
                    true,
                    true),
                new RoomOverview(
                    TheSecond<Room>().RoomNumber,
                    TheSecond<Room>().BedCount,
                    TheSecond<Room>().RoomCategory,
                    TheSecond<Room>().PricePerNight,
                    true,
                    false)]);
    }
}
