namespace MyHotel.Core.Spec.Rooms.RoomService;

public class WhenUpdateRoomWithZeroBeds : Spec<Core.Rooms.RoomService, RoomOverview>
{
    public WhenUpdateRoomWithZeroBeds() =>
        When(_ => _.Update(A<Room>().RoomNumber, The<Room>() with { BedCount = 0 }));

    [Fact]
    public void ThenThrowInvalidBedCount() =>
        Then().Throws<InvalidBedCount>().that.Message.Does().Contain("from 1 to 9 beds");
}
