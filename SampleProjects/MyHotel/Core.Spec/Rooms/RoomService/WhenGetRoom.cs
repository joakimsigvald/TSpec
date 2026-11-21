namespace MyHotel.Core.Spec.Rooms.RoomService;

public abstract class WhenGetRoom : Spec<Core.Rooms.RoomService, RoomOverview>
{
    protected WhenGetRoom() => When(_ => _.RoomFor(A<Room>().RoomNumber));

    public class GivenTheRoomIsStored : WhenGetRoom
    {
        public GivenTheRoomIsStored() =>
            Given<IRoomStore>().That(_ => _.Load()).Returns(One<Room>)
                .And<IRoomBookings>().Returns(Zero<string>);

        [Fact]
        public void ThenReturnIt() =>
            Result.Is(new RoomOverview(
                The<Room>().RoomNumber,
                The<Room>().BedCount,
                The<Room>().RoomCategory,
                The<Room>().PricePerNight,
                false,
                false));
    }

    public class GivenTheRoomIsHeld : WhenGetRoom
    {
        public GivenTheRoomIsHeld() =>
            Given<IRoomStore>().That(_ => _.Load()).Returns(One<Room>)
                .And<IRoomBookings>().That(_ => _.HeldRooms()).Returns(() => [The<Room>().RoomNumber]);

        [Fact] public void ThenSaySo() => Result.Held.Is(true);
    }

    public class GivenTheRoomHasCancellableBookings : WhenGetRoom
    {
        public GivenTheRoomHasCancellableBookings() =>
            Given<IRoomStore>().That(_ => _.Load()).Returns(One<Room>)
                .And<IRoomBookings>().That(_ => _.RoomsWithCancellableBookings())
                .Returns(() => [The<Room>().RoomNumber]);

        [Fact] public void ThenSaySo() => Result.HasCancellableBookings.Is(true);
    }

    public class GivenNoSuchRoom : WhenGetRoom
    {
        public GivenNoSuchRoom() => Given<IRoomStore>().That(_ => _.Load()).Returns(Zero<Room>);

        [Fact]
        public void ThenThrowNotFound() =>
            Then().Throws<RoomNotFound>().that.Message.Does().Contain(The<Room>().RoomNumber);
    }
}
