namespace MyHotel.Core.Spec.Rooms.RoomService;

public class WhenUpdateRoomToZeroKronor : Spec<Core.Rooms.RoomService, RoomOverview>
{
    public WhenUpdateRoomToZeroKronor() =>
        When(_ => _.Update(A<Room>().RoomNumber, The<Room>() with { PricePerNight = 0 }));

    [Fact]
    public void ThenThrowInvalidPricePerNight() =>
        Then().Throws<InvalidPricePerNight>().that.Message.Does().Contain("cannot be 0 kr");
}
