namespace MyHotel.Core.Spec.Rooms.RoomService;

public abstract class WhenListRooms : Spec<Core.Rooms.RoomService, IReadOnlyList<RoomOverview>>
{
    protected WhenListRooms() => When(_ => _.Rooms());

    public class GivenNoRooms : WhenListRooms
    {
        public GivenNoRooms() => Given<IRoomStore>().That(_ => _.Load()).Returns(Zero<Room>);

        [Fact] public void ThenReturnNoRooms() => Result.Is().Empty();
    }

    public class GivenTwoRooms : WhenListRooms
    {
        public GivenTwoRooms() =>
            Given<IRoomStore>().That(_ => _.Load()).Returns(Two<Room>)
                .And<IOpenBookings>().That(_ => _.RoomNumbers())
                .Returns(() => [TheSecond<Room>().RoomNumber]);

        [Fact]
        public void ThenReturnThemInTheOrderTheyAreStored() =>
            Result.Is().EqualTo([
                new RoomOverview(TheFirst<Room>().RoomNumber, TheFirst<Room>().BedCount, false),
                new RoomOverview(TheSecond<Room>().RoomNumber, TheSecond<Room>().BedCount, true)]);
    }
}
