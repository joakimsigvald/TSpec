namespace MyHotel.Core.Spec.Rooms.RoomService;

public class WhenAddRoomAtZeroKronor : Spec<Core.Rooms.RoomService, RoomOverview>
{
    public WhenAddRoomAtZeroKronor() => When(_ => _.Add(A<Room>() with { PricePerNight = 0 }));

    [Fact]
    public void ThenThrowInvalidPricePerNight() =>
        Then().Throws<InvalidPricePerNight>().that.Message.Does().Contain("cannot be 0 kr");
}
