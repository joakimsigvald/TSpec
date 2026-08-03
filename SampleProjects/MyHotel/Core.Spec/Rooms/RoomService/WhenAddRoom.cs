namespace MyHotel.Core.Spec.Rooms.RoomService;

public abstract class WhenAddRoom : Spec<Core.Rooms.RoomService, Room>
{
    protected WhenAddRoom() => When(_ => _.Add(A<Room>()));

    public class GivenNoSuchRoom : WhenAddRoom
    {
        public GivenNoSuchRoom() => Given<IRoomStore>().That(_ => _.Load()).Returns(Zero<Room>);

        [Fact] public void ThenReturnTheRoom() => Result.Is(The<Room>());

        [Fact] public void ThenStoreIt() => Then<IRoomStore>(_ => _.Save(One<Room>()));
    }

    /// <summary>
    /// A different room carrying the same room number, so the number alone is what is refused.
    /// </summary>
    public class GivenTheRoomNumberIsTaken : WhenAddRoom
    {
        public GivenTheRoomNumberIsTaken()
            => Given<IRoomStore>().That(_ => _.Load())
                .Returns(() => [The<Room>() with { BedCount = Any<int>() }]);

        [Fact]
        public void ThenThrowRoomAlreadyExists()
            => Then().Throws<RoomAlreadyExists>().that.Message.Does().Contain(The<Room>().RoomNumber);
    }
}
