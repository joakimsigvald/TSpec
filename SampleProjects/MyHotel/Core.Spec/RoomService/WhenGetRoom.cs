namespace MyHotel.Core.Spec.RoomService;

public abstract class WhenGetRoom : Spec<Core.RoomService, Room>
{
    protected WhenGetRoom() => When(_ => _.Get(A<Room>().RoomNumber));

    public class GivenTheRoomIsStored : WhenGetRoom
    {
        public GivenTheRoomIsStored() => Given<IRoomStore>().That(_ => _.Load()).Returns(One<Room>);

        [Fact] public void ThenReturnIt() => Result.Is(The<Room>());
    }

    public class GivenNoSuchRoom : WhenGetRoom
    {
        public GivenNoSuchRoom() => Given<IRoomStore>().That(_ => _.Load()).Returns(Zero<Room>);

        [Fact] public void ThenThrowNotFound() => Then().Throws<RoomNotFound>();
    }
}
