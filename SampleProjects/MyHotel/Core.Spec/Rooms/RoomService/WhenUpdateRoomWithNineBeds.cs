namespace MyHotel.Core.Spec.Rooms.RoomService;

public abstract class WhenUpdateRoomWithNineBeds : Spec<Core.Rooms.RoomService, RoomOverview>
{
    protected WhenUpdateRoomWithNineBeds() =>
        When(_ => _.Update(A<Room>().RoomNumber, The<Room>() with { BedCount = 9 }));

    public class GivenTheRoomIsStored : WhenUpdateRoomWithNineBeds
    {
        public GivenTheRoomIsStored() =>
            Given<IRoomStore>().That(_ => _.Load()).Returns(One<Room>)
                .And<IRoomCategoryStore>().That(_ => _.Load())
                .Returns(() => [A<RoomCategory>() with { Name = The<Room>().RoomCategory }])
                .And<IRoomBookings>().Returns(Zero<string>);

        [Fact] public void ThenReturnTheRoomWithNineBeds() => Result.BedCount.Is(9);
    }
}
