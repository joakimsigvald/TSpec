namespace MyHotel.Core.Spec.Rooms.RoomService;

public abstract class WhenAddRoomWithOneBed : Spec<Core.Rooms.RoomService, RoomOverview>
{
    protected WhenAddRoomWithOneBed() => When(_ => _.Add(A<Room>() with { BedCount = 1 }));

    public class GivenNoSuchRoom : WhenAddRoomWithOneBed
    {
        public GivenNoSuchRoom() =>
            Given<IRoomStore>().That(_ => _.Load()).Returns(Zero<Room>)
                .And<IRoomCategoryStore>().That(_ => _.Load())
                .Returns(() => [new RoomCategory(The<Room>().RoomCategory, Any<string>())])
                .And<IOpenBookings>().That(_ => _.RoomNumbers()).Returns(Zero<string>);

        [Fact] public void ThenReturnTheRoomWithOneBed() => Result.BedCount.Is(1);
    }
}
