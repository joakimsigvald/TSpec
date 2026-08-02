namespace MyHotel.Core.Spec.RoomService;

/// <summary>
/// The room number identifies the room, so the update carries the room it becomes rather than a
/// number of its own. A tag names that room, and each branch says which one it is.
/// </summary>
public abstract class WhenUpdateRoom : Spec<Core.RoomService, Room>
{
    private readonly Tag<Room> _updatedRoom = new();

    protected WhenUpdateRoom() => When(_ => _.Update(A<Room>().RoomNumber, The(_updatedRoom)));

    public class GivenTheRoomIsStored : WhenUpdateRoom
    {
        public GivenTheRoomIsStored()
        {
            Given(_updatedRoom).Is(The<Room>() with { BedCount = ASecond<int>() });
            Given<IRoomStore>().That(_ => _.Load()).Returns(One<Room>);
        }

        [Fact] public void ThenReturnTheUpdatedRoom() => Result.Is(The(_updatedRoom));
    }

    /// <summary>
    /// The updated room replaces the old one where it stood, so an update is not a delete and an add.
    /// </summary>
    public class GivenAnotherRoomIsStoredFirst : WhenUpdateRoom
    {
        public GivenAnotherRoomIsStoredFirst()
        {
            Given(_updatedRoom).Is(The<Room>() with { BedCount = ASecond<int>() });
            Given<IRoomStore>().That(_ => _.Load()).Returns(() => [ASecond<Room>(), The<Room>()]);
        }

        [Fact]
        public void ThenKeepItWhereItWas()
            => Then<IRoomStore>(_ => _.Save(new[] { TheSecond<Room>(), The(_updatedRoom) }));
    }

    public class GivenNoSuchRoom : WhenUpdateRoom
    {
        public GivenNoSuchRoom()
        {
            Given(_updatedRoom).Is(The<Room>() with { BedCount = ASecond<int>() });
            Given<IRoomStore>().That(_ => _.Load()).Returns(Zero<Room>);
        }

        [Fact]
        public void ThenThrowNotFound()
            => Then().Throws<RoomNotFound>().that.Message.Does().Contain(The<Room>().RoomNumber);
    }

    /// <summary>
    /// Renumbering a room means deleting it and adding another, so the update refuses to do it —
    /// before it even looks for the room, since the request is wrong whether or not it exists.
    /// </summary>
    public class GivenTheUpdatedRoomHasAnotherNumber : WhenUpdateRoom
    {
        public GivenTheUpdatedRoomHasAnotherNumber() => Given(_updatedRoom).Is(ASecond<Room>());

        [Fact]
        public void ThenThrowRoomNumberMismatch()
            => Then().Throws<RoomNumberMismatch>().that.Message.Does().Contain(The(_updatedRoom).RoomNumber);
    }
}
