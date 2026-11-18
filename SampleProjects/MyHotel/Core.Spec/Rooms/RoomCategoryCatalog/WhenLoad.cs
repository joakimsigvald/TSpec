namespace MyHotel.Core.Spec.Rooms.RoomCategoryCatalog;

public abstract class WhenLoad : Spec<Core.Rooms.RoomCategoryCatalog, IReadOnlyList<RoomCategory>>
{
    protected WhenLoad() => When(_ => _.Load());

    public class GivenNothingIsStored : WhenLoad
    {
        public GivenNothingIsStored() =>
            Given<IRoomCategoryStore>().That(_ => _.Load()).Returns(Zero<RoomCategory>);

        [Fact]
        public void ThenReturnStandard() =>
            Result.Is().EqualTo([new RoomCategory("Standard", "Standardrum")]);
    }

    public class GivenTwoAreStored : WhenLoad
    {
        public GivenTwoAreStored() =>
            Given<IRoomCategoryStore>().That(_ => _.Load()).Returns(Two<RoomCategory>);

        [Fact] public void ThenReturnThem() => Result.Is().EqualTo(Two<RoomCategory>());
    }
}
