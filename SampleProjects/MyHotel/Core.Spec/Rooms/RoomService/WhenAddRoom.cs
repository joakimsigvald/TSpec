using static TSpec.Times;

namespace MyHotel.Core.Spec.Rooms.RoomService;

public abstract class WhenAddRoom : Spec<Core.Rooms.RoomService, RoomOverview>
{
    protected WhenAddRoom() => When(_ => _.Add(A<Room>()));

    public class GivenNoSuchRoom : WhenAddRoom
    {
        public GivenNoSuchRoom() =>
            Given<IRoomStore>().That(_ => _.Load()).Returns(Zero<Room>)
                .And<IRoomCategoryStore>().That(_ => _.Load())
                .Returns(() => [A<RoomCategory>() with { Name = The<Room>().RoomCategory }])
                .And<IRoomBookings>().Returns(Zero<string>);

        [Fact]
        public void ThenReturnTheRoom() =>
            Result.Is(new RoomOverview(
                The<Room>().RoomNumber,
                The<Room>().BedCount,
                The<Room>().RoomCategory,
                The<Room>().PricePerNight,
                false,
                false));

        [Fact] public void ThenStoreIt() => Then<IRoomStore>(_ => _.Save(One<Room>()));
    }

    /// <summary>
    /// A different room carrying the same room number, so the number alone is what is refused.
    /// </summary>
    public class GivenTheRoomNumberIsTaken : WhenAddRoom
    {
        public GivenTheRoomNumberIsTaken() =>
            Given<IRoomStore>().That(_ => _.Load())
                .Returns(() => [The<Room>() with { BedCount = Any<int>() }]);

        [Fact]
        public void ThenThrowRoomAlreadyExists() =>
            Then().Throws<RoomAlreadyExists>().that.Message.Does().Contain(The<Room>().RoomNumber);
    }

    public class GivenNoSuchRoomCategory : WhenAddRoom
    {
        public GivenNoSuchRoomCategory() =>
            Given<IRoomStore>().That(_ => _.Load()).Returns(Zero<Room>)
                .And<IRoomCategoryStore>().That(_ => _.Load()).Returns(One<RoomCategory>);

        [Fact]
        public void ThenThrowRoomCategoryNotFound() =>
            Then().Throws<RoomCategoryNotFound>().that.Message.Does().Contain(The<Room>().RoomCategory);

        [Fact] public void ThenStoreNothing() => Then<IRoomStore>(nameof(IRoomStore.Save), Never);
    }
}
