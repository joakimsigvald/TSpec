using MyHotel.Contract;

namespace MyHotel.Core.Rooms;

/// <summary>
/// Rooms are kept in the order they were created, which is the order the store returns them in.
/// </summary>
public class RoomService(IRoomStore store) : IRoomService
{
    public async Task<IReadOnlyList<Room>> List() => await store.Load();

    public async Task<Room> Get(string roomNumber) => Existing(await store.Load(), roomNumber);

    public async Task<Room> Add(Room room)
    {
        var rooms = await Rooms();
        if (Find(rooms, room.RoomNumber) is not null)
            throw new RoomAlreadyExists(room.RoomNumber);
        rooms.Add(room);
        await store.Save(rooms);
        return room;
    }

    public async Task<Room> Update(string roomNumber, Room room)
    {
        if (room.RoomNumber != roomNumber)
            throw new RoomNumberMismatch(roomNumber, room.RoomNumber);
        var rooms = await Rooms();
        rooms[rooms.IndexOf(Existing(rooms, roomNumber))] = room;
        await store.Save(rooms);
        return room;
    }

    public async Task Delete(string roomNumber)
    {
        var rooms = await Rooms();
        rooms.Remove(Existing(rooms, roomNumber));
        await store.Save(rooms);
    }

    private async Task<List<Room>> Rooms() => [.. await store.Load()];

    private static Room Existing(IEnumerable<Room> rooms, string roomNumber)
        => Find(rooms, roomNumber) ?? throw new RoomNotFound(roomNumber);

    private static Room? Find(IEnumerable<Room> rooms, string roomNumber)
        => rooms.FirstOrDefault(room => room.RoomNumber == roomNumber);
}
