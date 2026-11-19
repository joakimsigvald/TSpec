namespace MyHotel.Core.Spec.Rooms.RoomService;

public class WhenAddRoomWithZeroBeds : Spec<Core.Rooms.RoomService, RoomOverview>
{
    public WhenAddRoomWithZeroBeds() => When(_ => _.Add(A<Room>() with { BedCount = 0 }));

    [Fact]
    public void ThenThrowInvalidBedCount() =>
        Then().Throws<InvalidBedCount>().that.Message.Does().Contain("from 1 to 9 beds");
}
