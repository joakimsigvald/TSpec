namespace MyHotel.Core.Spec.Rooms.RoomService;

public abstract class WhenAddRoomAtOneKrona : Spec<Core.Rooms.RoomService, RoomOverview>
{
    protected WhenAddRoomAtOneKrona() => When(_ => _.Add(A<Room>() with { PricePerNight = 1 }));

    public class GivenNoSuchRoom : WhenAddRoomAtOneKrona
    {
        public GivenNoSuchRoom() =>
            Given<IRoomStore>().That(_ => _.Load()).Returns(Zero<Room>)
                .And<IRoomCategoryStore>().That(_ => _.Load())
                .Returns(() => [A<RoomCategory>() with { Name = The<Room>().RoomCategory }])
                .And<IRoomBookings>().Returns(Zero<string>);

        [Fact] public void ThenReturnTheRoomAtOneKrona() => Result.PricePerNight.Is(1);
    }
}
