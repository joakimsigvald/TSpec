namespace MyHotel.Core.Spec.Rooms.RoomCategoryService;

public abstract class WhenAddRoomCategory : Spec<Core.Rooms.RoomCategoryService, RoomCategoryOverview>
{
    protected WhenAddRoomCategory() => When(_ => _.Add(A<RoomCategory>()));

    public class GivenNoSuchRoomCategory : WhenAddRoomCategory
    {
        public GivenNoSuchRoomCategory() =>
            Given<IRoomCategoryStore>().That(_ => _.Load()).Returns(() => [ASecond<RoomCategory>()])
                .And<IRoomStore>().That(_ => _.Load()).Returns(Zero<Room>);

        [Fact]
        public void ThenReturnTheRoomCategory() =>
            Result.Is(new RoomCategoryOverview(
                The<RoomCategory>().Name, The<RoomCategory>().Description, false));

        [Fact]
        public void ThenStoreItLast() =>
            Then<IRoomCategoryStore>(_ => _.Save(new[] { TheSecond<RoomCategory>(), The<RoomCategory>() }));
    }

    public class GivenANewHotel : WhenAddRoomCategory
    {
        public GivenANewHotel() =>
            Given<IRoomCategoryStore>().That(_ => _.Load()).Returns(Zero<RoomCategory>)
                .And<IRoomStore>().That(_ => _.Load()).Returns(Zero<Room>);

        [Fact]
        public void ThenStoreStandardWithIt() =>
            Then<IRoomCategoryStore>(_ => _.Save(
                new[] { new RoomCategory("Standard", "Standardrum"), The<RoomCategory>() }));
    }

    /// <summary>
    /// A different category carrying the same name, so the name alone is what is refused.
    /// </summary>
    public class GivenTheNameIsTaken : WhenAddRoomCategory
    {
        public GivenTheNameIsTaken() =>
            Given<IRoomCategoryStore>().That(_ => _.Load())
                .Returns(() => [The<RoomCategory>() with { Description = Any<string>() }]);

        [Fact]
        public void ThenThrowRoomCategoryAlreadyExists() =>
            Then().Throws<RoomCategoryAlreadyExists>().that.Message
                .Does().Contain(The<RoomCategory>().Name);
    }
}
