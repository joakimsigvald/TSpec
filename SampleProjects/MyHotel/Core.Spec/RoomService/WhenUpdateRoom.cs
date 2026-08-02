namespace MyHotel.Core.Spec.RoomService;

public abstract class WhenUpdateRoom : Spec<Core.RoomService, Room>
{
    protected WhenUpdateRoom() => When(_ => _.Update(A<Room>().RoomNumber, ASecond<Room>()));

    public class GivenTheRoomIsStored : WhenUpdateRoom
    {
        public GivenTheRoomIsStored() => Given<IRoomStore>().That(_ => _.Load()).Returns(One<Room>);

        [Fact] public void ThenReturnTheNewRoom() => Result.Is(ASecond<Room>());
    }

    /// <summary>
    /// The new room replaces the old one where it stood, so an update is not a delete and an add.
    /// </summary>
    public class GivenAnotherRoomIsStoredFirst : WhenUpdateRoom
    {
        public GivenAnotherRoomIsStoredFirst()
            => Given<IRoomStore>().That(_ => _.Load())
                .Returns(() => [AThird<Room>(), The<Room>()]);

        [Fact]
        public void ThenKeepItWhereItWas()
            => Then<IRoomStore>(_ => _.Save(new[] { TheThird<Room>(), TheSecond<Room>() }));
    }

    public class GivenNoSuchRoom : WhenUpdateRoom
    {
        public GivenNoSuchRoom() => Given<IRoomStore>().That(_ => _.Load()).Returns(Zero<Room>);

        [Fact] public void ThenThrowNotFound() => Then().Throws<RoomNotFound>();
    }
}
