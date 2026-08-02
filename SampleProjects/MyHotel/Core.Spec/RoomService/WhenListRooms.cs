namespace MyHotel.Core.Spec.RoomService;

public abstract class WhenListRooms : Spec<Core.RoomService, IReadOnlyList<Room>>
{
    protected WhenListRooms() => When(_ => _.List());

    public class GivenNoRooms : WhenListRooms
    {
        public GivenNoRooms() => Given<IRoomStore>().That(_ => _.Load()).Returns(Zero<Room>);

        [Fact] public void ThenReturnNoRooms() => Result.Is().Empty();
    }

    public class GivenTwoRooms : WhenListRooms
    {
        public GivenTwoRooms() => Given<IRoomStore>().That(_ => _.Load()).Returns(Two<Room>);

        [Fact]
        public void ThenReturnThemInTheOrderTheyAreStored() => Result.Is().EqualTo(Two<Room>());
    }
}
