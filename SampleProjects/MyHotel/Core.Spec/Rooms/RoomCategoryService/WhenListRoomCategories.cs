namespace MyHotel.Core.Spec.Rooms.RoomCategoryService;

public abstract class WhenListRoomCategories
    : Spec<Core.Rooms.RoomCategoryService, IReadOnlyList<RoomCategoryOverview>>
{
    protected WhenListRoomCategories() => When(_ => _.RoomCategories());

    public class GivenTwoRoomCategories : WhenListRoomCategories
    {
        public GivenTwoRoomCategories() =>
            Given<IRoomCategoryStore>().That(_ => _.Load()).Returns(Two<RoomCategory>)
                .And<IRoomStore>().That(_ => _.Load())
                .Returns(() => [A<Room>() with { RoomCategory = TheSecond<RoomCategory>().Name }]);

        [Fact]
        public void ThenReturnThemInTheOrderTheyAreStored() =>
            Result.Is().EqualTo([
                new RoomCategoryOverview(
                    TheFirst<RoomCategory>().Name, TheFirst<RoomCategory>().Description, false),
                new RoomCategoryOverview(
                    TheSecond<RoomCategory>().Name, TheSecond<RoomCategory>().Description, true)]);
    }
}
