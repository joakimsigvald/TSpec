namespace MyHotel.Core.Spec.Rooms.RoomService;

public abstract class WhenGetRoom : Spec<Core.Rooms.RoomService, RoomOverview>
{
    protected WhenGetRoom() => When(_ => _.RoomFor(A<Room>().RoomNumber));

    public class GivenTheRoomIsStored : WhenGetRoom
    {
        public GivenTheRoomIsStored() =>
            Given<IRoomStore>().That(_ => _.Load()).Returns(One<Room>)
                .And<IOpenBookings>().That(_ => _.RoomNumbers()).Returns(Zero<string>);

        [Fact]
        public void ThenReturnIt() =>
            Result.Is(new RoomOverview(
                The<Room>().RoomNumber, The<Room>().BedCount, The<Room>().RoomCategory, false));
    }

    public class GivenTheRoomHasOpenBookings : WhenGetRoom
    {
        public GivenTheRoomHasOpenBookings() =>
            Given<IRoomStore>().That(_ => _.Load()).Returns(One<Room>)
                .And<IOpenBookings>().That(_ => _.RoomNumbers()).Returns(() => [The<Room>().RoomNumber]);

        [Fact] public void ThenSaySo() => Result.HasOpenBookings.Is(true);
    }

    public class GivenNoSuchRoom : WhenGetRoom
    {
        public GivenNoSuchRoom() => Given<IRoomStore>().That(_ => _.Load()).Returns(Zero<Room>);

        [Fact]
        public void ThenThrowNotFound() =>
            Then().Throws<RoomNotFound>().that.Message.Does().Contain(The<Room>().RoomNumber);
    }
}
