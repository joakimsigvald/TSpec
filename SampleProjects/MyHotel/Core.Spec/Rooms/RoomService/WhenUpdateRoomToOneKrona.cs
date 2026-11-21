namespace MyHotel.Core.Spec.Rooms.RoomService;

public abstract class WhenUpdateRoomToOneKrona : Spec<Core.Rooms.RoomService, RoomOverview>
{
    protected WhenUpdateRoomToOneKrona() =>
        When(_ => _.Update(A<Room>().RoomNumber, The<Room>() with { PricePerNight = 1 }));

    public class GivenTheRoomIsStored : WhenUpdateRoomToOneKrona
    {
        public GivenTheRoomIsStored() =>
            Given<IRoomStore>().That(_ => _.Load()).Returns(One<Room>)
                .And<IRoomCategoryStore>().That(_ => _.Load())
                .Returns(() => [A<RoomCategory>() with { Name = The<Room>().RoomCategory }])
                .And<IRoomBookings>().Returns(Zero<string>);

        [Fact] public void ThenReturnTheRoomAtOneKrona() => Result.PricePerNight.Is(1);
    }
}
