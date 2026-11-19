namespace MyHotel.Core.Spec.Rooms.RoomService;

public abstract class WhenUpdateRoomWithTenBeds : Spec<Core.Rooms.RoomService, RoomOverview>
{
    protected WhenUpdateRoomWithTenBeds() =>
        When(_ => _.Update(A<Room>().RoomNumber, The<Room>() with { BedCount = 10 }));

    public class GivenNoSuchRoom : WhenUpdateRoomWithTenBeds
    {
        public GivenNoSuchRoom() => Given<IRoomStore>().That(_ => _.Load()).Returns(Zero<Room>);

        [Fact]
        public void ThenThrowInvalidBedCount() =>
            Then().Throws<InvalidBedCount>().that.Message.Does().Contain("from 1 to 9 beds");
    }
}
