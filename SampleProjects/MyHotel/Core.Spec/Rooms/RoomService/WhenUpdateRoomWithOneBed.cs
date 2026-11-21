namespace MyHotel.Core.Spec.Rooms.RoomService;

public abstract class WhenUpdateRoomWithOneBed : Spec<Core.Rooms.RoomService, RoomOverview>
{
    protected WhenUpdateRoomWithOneBed() =>
        When(_ => _.Update(A<Room>().RoomNumber, The<Room>() with { BedCount = 1 }));

    public class GivenTheRoomIsStored : WhenUpdateRoomWithOneBed
    {
        public GivenTheRoomIsStored() =>
            Given<IRoomStore>().That(_ => _.Load()).Returns(One<Room>)
                .And<IRoomCategoryStore>().That(_ => _.Load())
                .Returns(() => [A<RoomCategory>() with { Name = The<Room>().RoomCategory }])
                .And<IRoomBookings>().Returns(Zero<string>);

        [Fact] public void ThenReturnTheRoomWithOneBed() => Result.BedCount.Is(1);
    }
}
