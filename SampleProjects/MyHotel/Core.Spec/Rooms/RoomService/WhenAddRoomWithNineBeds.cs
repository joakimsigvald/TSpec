namespace MyHotel.Core.Spec.Rooms.RoomService;

public abstract class WhenAddRoomWithNineBeds : Spec<Core.Rooms.RoomService, RoomOverview>
{
    protected WhenAddRoomWithNineBeds() => When(_ => _.Add(A<Room>() with { BedCount = 9 }));

    public class GivenNoSuchRoom : WhenAddRoomWithNineBeds
    {
        public GivenNoSuchRoom() =>
            Given<IRoomStore>().That(_ => _.Load()).Returns(Zero<Room>)
                .And<IRoomCategoryStore>().That(_ => _.Load())
                .Returns(() => [new RoomCategory(The<Room>().RoomCategory, Any<string>())])
                .And<IOpenBookings>().That(_ => _.RoomNumbers()).Returns(Zero<string>);

        [Fact] public void ThenReturnTheRoomWithNineBeds() => Result.BedCount.Is(9);
    }
}
