using MyHotel.Contract;

namespace MyHotel.Core;

/// <summary>
/// Rooms held in a list, because they are listed in the order they were created. Nothing here
/// actually waits — the methods are async because the interface is, so that a real store can
/// replace this one without the endpoints changing.
/// </summary>
public class RoomService : IRoomService
{
    private readonly List<Room> _rooms = [];

    public Task<IReadOnlyList<Room>> List() => Task.FromResult<IReadOnlyList<Room>>(_rooms);

    public Task<Room> Get(string roomNumber) => Task.FromResult(Existing(roomNumber));

    public Task<Room> Add(Room room)
    {
        if (Find(room.RoomNumber) is not null)
            throw new RoomAlreadyExists(room.RoomNumber);
        _rooms.Add(room);
        return Task.FromResult(room);
    }

    public Task<Room> Update(string roomNumber, Room room)
    {
        _rooms[_rooms.IndexOf(Existing(roomNumber))] = room;
        return Task.FromResult(room);
    }

    public Task Delete(string roomNumber)
    {
        _rooms.Remove(Existing(roomNumber));
        return Task.CompletedTask;
    }

    private Room Existing(string roomNumber)
        => Find(roomNumber) ?? throw new RoomNotFound(roomNumber);

    private Room? Find(string roomNumber) => _rooms.Find(room => room.RoomNumber == roomNumber);
}
