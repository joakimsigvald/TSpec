namespace MyHotel.Core.Spec.Rooms.RoomService;

public abstract class WhenAddRoomWithTenBeds : Spec<Core.Rooms.RoomService, RoomOverview>
{
    protected WhenAddRoomWithTenBeds() => When(_ => _.Add(A<Room>() with { BedCount = 10 }));

    public class GivenTheRoomNumberIsTaken : WhenAddRoomWithTenBeds
    {
        public GivenTheRoomNumberIsTaken() => Given<IRoomStore>().That(_ => _.Load()).Returns(One<Room>);

        [Fact]
        public void ThenThrowInvalidBedCount() =>
            Then().Throws<InvalidBedCount>().that.Message.Does().Contain("from 1 to 9 beds");
    }
}
