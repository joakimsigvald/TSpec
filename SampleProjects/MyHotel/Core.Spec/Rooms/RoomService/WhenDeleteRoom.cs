using static TSpec.Times;

namespace MyHotel.Core.Spec.Rooms.RoomService;

public abstract class WhenDeleteRoom : Spec<Core.Rooms.RoomService>
{
    protected WhenDeleteRoom() => When(_ => _.Delete(A<Room>().RoomNumber));

    public class GivenTheRoomIsStored : WhenDeleteRoom
    {
        public GivenTheRoomIsStored() =>
            Given<IRoomStore>().That(_ => _.Load()).Returns(One<Room>)
                .And<IOpenBookings>().That(_ => _.RoomNumbers()).Returns(Zero<string>);

        [Fact] public void ThenStoreWhatIsLeft() => Then<IRoomStore>(_ => _.Save(Zero<Room>()));
    }

    public class GivenTheRoomHasOpenBookings : WhenDeleteRoom
    {
        public GivenTheRoomHasOpenBookings() =>
            Given<IRoomStore>().That(_ => _.Load()).Returns(One<Room>)
                .And<IOpenBookings>().That(_ => _.RoomNumbers()).Returns(() => [The<Room>().RoomNumber]);

        [Fact]
        public void ThenThrowRoomHasOpenBookings() =>
            Then().Throws<RoomHasOpenBookings>().that.Message.Does().Contain(The<Room>().RoomNumber);

        [Fact] public void ThenStoreNothing() => Then<IRoomStore>(nameof(IRoomStore.Save), Never);
    }

    public class GivenNoSuchRoom : WhenDeleteRoom
    {
        public GivenNoSuchRoom() => Given<IRoomStore>().That(_ => _.Load()).Returns(Zero<Room>);

        [Fact]
        public void ThenThrowNotFound() =>
            Then().Throws<RoomNotFound>().that.Message.Does().Contain(The<Room>().RoomNumber);
    }
}
