namespace MyHotel.Core.Spec.RoomService;

public abstract class WhenDeleteRoom : Spec<Core.RoomService>
{
    protected WhenDeleteRoom() => When(_ => _.Delete(A<Room>().RoomNumber));

    public class GivenTheRoomIsStored : WhenDeleteRoom
    {
        public GivenTheRoomIsStored()
            => Given<IRoomStore>().That(_ => _.Load()).Returns(One<Room>);

        [Fact] public void ThenStoreWhatIsLeft() => Then<IRoomStore>(_ => _.Save(Zero<Room>()));
    }

    public class GivenNoSuchRoom : WhenDeleteRoom
    {
        public GivenNoSuchRoom() => Given<IRoomStore>().That(_ => _.Load()).Returns(Zero<Room>);

        [Fact]
        public void ThenThrowNotFound()
            => Then().Throws<RoomNotFound>().that.Message.Does().Contain(The<Room>().RoomNumber);
    }
}
