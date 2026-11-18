namespace MyHotel.Core.Spec.Rooms.RoomCategoryService;

/// <summary>
/// The name identifies the room category, so the update carries the category it becomes. A tag
/// names that category, and each branch says which one it is.
/// </summary>
public abstract class WhenUpdateRoomCategory : Spec<Core.Rooms.RoomCategoryService, RoomCategoryOverview>
{
    private readonly Tag<RoomCategory> _updatedRoomCategory = new();

    protected WhenUpdateRoomCategory() =>
        When(_ => _.Update(A<RoomCategory>().Name, The(_updatedRoomCategory)));

    public class GivenTheRoomCategoryIsStored : WhenUpdateRoomCategory
    {
        public GivenTheRoomCategoryIsStored()
        {
            Given(_updatedRoomCategory).Is(The<RoomCategory>() with { Description = ASecond<string>() });
            Given<IRoomCategoryStore>().That(_ => _.Load()).Returns(One<RoomCategory>)
                .And<IRoomStore>().That(_ => _.Load()).Returns(Zero<Room>);
        }

        [Fact]
        public void ThenReturnTheUpdatedRoomCategory() =>
            Result.Is(new RoomCategoryOverview(
                The(_updatedRoomCategory).Name, The(_updatedRoomCategory).Description, false));
    }

    public class GivenAnotherRoomCategoryIsStoredFirst : WhenUpdateRoomCategory
    {
        public GivenAnotherRoomCategoryIsStoredFirst()
        {
            Given(_updatedRoomCategory).Is(The<RoomCategory>() with { Description = ASecond<string>() });
            Given<IRoomCategoryStore>().That(_ => _.Load())
                .Returns(() => [ASecond<RoomCategory>(), The<RoomCategory>()]);
        }

        [Fact]
        public void ThenKeepItWhereItWas() =>
            Then<IRoomCategoryStore>(_ => _.Save(
                new[] { TheSecond<RoomCategory>(), The(_updatedRoomCategory) }));
    }

    public class GivenNoSuchRoomCategory : WhenUpdateRoomCategory
    {
        public GivenNoSuchRoomCategory()
        {
            Given(_updatedRoomCategory).Is(The<RoomCategory>() with { Description = ASecond<string>() });
            Given<IRoomCategoryStore>().That(_ => _.Load()).Returns(() => [ASecond<RoomCategory>()]);
        }

        [Fact]
        public void ThenThrowNotFound() =>
            Then().Throws<RoomCategoryNotFound>().that.Message.Does().Contain(The<RoomCategory>().Name);
    }

    public class GivenTheUpdatedRoomCategoryHasAnotherName : WhenUpdateRoomCategory
    {
        public GivenTheUpdatedRoomCategoryHasAnotherName() =>
            Given(_updatedRoomCategory).Is(ASecond<RoomCategory>());

        [Fact]
        public void ThenThrowRoomCategoryNameMismatch() =>
            Then().Throws<RoomCategoryNameMismatch>().that.Message
                .Does().Contain(The(_updatedRoomCategory).Name);
    }
}
