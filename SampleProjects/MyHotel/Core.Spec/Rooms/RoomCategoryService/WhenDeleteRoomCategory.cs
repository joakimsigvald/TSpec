using static TSpec.Times;

namespace MyHotel.Core.Spec.Rooms.RoomCategoryService;

public abstract class WhenDeleteRoomCategory : Spec<Core.Rooms.RoomCategoryService>
{
    protected WhenDeleteRoomCategory() => When(_ => _.Delete(A<RoomCategory>().Name));

    public class GivenAnotherRoomCategoryIsStored : WhenDeleteRoomCategory
    {
        public GivenAnotherRoomCategoryIsStored() =>
            Given<IRoomCategoryStore>().That(_ => _.Load())
                .Returns(() => [The<RoomCategory>(), ASecond<RoomCategory>()])
                .And<IRoomStore>().That(_ => _.Load()).Returns(Zero<Room>);

        [Fact]
        public void ThenStoreWhatIsLeft() =>
            Then<IRoomCategoryStore>(_ => _.Save(new[] { TheSecond<RoomCategory>() }));
    }

    public class GivenARoomHasIt : WhenDeleteRoomCategory
    {
        public GivenARoomHasIt() =>
            Given<IRoomCategoryStore>().That(_ => _.Load())
                .Returns(() => [The<RoomCategory>(), ASecond<RoomCategory>()])
                .And<IRoomStore>().That(_ => _.Load())
                .Returns(() => [A<Room>() with { RoomCategory = The<RoomCategory>().Name }]);

        [Fact]
        public void ThenThrowRoomCategoryHasRooms() =>
            Then().Throws<RoomCategoryHasRooms>().that.Message.Does().Contain(The<RoomCategory>().Name);

        [Fact]
        public void ThenStoreNothing() => Then<IRoomCategoryStore>(nameof(IRoomCategoryStore.Save), Never);
    }

    public class GivenItIsTheLast : WhenDeleteRoomCategory
    {
        public GivenItIsTheLast() =>
            Given<IRoomCategoryStore>().That(_ => _.Load()).Returns(One<RoomCategory>)
                .And<IRoomStore>().That(_ => _.Load()).Returns(Zero<Room>);

        [Fact]
        public void ThenThrowRoomCategoryIsLast() =>
            Then().Throws<RoomCategoryIsLast>().that.Message.Does().Contain(The<RoomCategory>().Name);

        [Fact]
        public void ThenStoreNothing() => Then<IRoomCategoryStore>(nameof(IRoomCategoryStore.Save), Never);
    }

    public class GivenNoSuchRoomCategory : WhenDeleteRoomCategory
    {
        public GivenNoSuchRoomCategory() =>
            Given<IRoomCategoryStore>().That(_ => _.Load()).Returns(() => [ASecond<RoomCategory>()]);

        [Fact]
        public void ThenThrowNotFound() =>
            Then().Throws<RoomCategoryNotFound>().that.Message.Does().Contain(The<RoomCategory>().Name);
    }
}
