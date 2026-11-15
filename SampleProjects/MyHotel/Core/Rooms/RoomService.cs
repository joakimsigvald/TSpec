using MyHotel.Contract.Rooms;
using MyHotel.Contract.Rooms.Exceptions;

namespace MyHotel.Core.Rooms;

/// <summary>
/// Rooms are kept in the order they were created, which is the order the store returns them in.
/// </summary>
public class RoomService(IRoomStore store) : IRoomService
{
    public async Task<IReadOnlyList<Room>> Rooms() => await store.Load();
    public async Task<Room> RoomFor(string roomNumber) => Existing(await Rooms(), roomNumber);

    public async Task<Room> Add(Room room)
    {
        var rooms = await Rooms();
        if (Find(rooms, room.RoomNumber) is not null)
            throw new RoomAlreadyExists(room.RoomNumber);
        await store.Save([.. rooms, room]);
        return room;
    }

    public async Task<Room> Update(string roomNumber, Room room)
    {
        if (room.RoomNumber != roomNumber)
            throw new RoomNumberMismatch(roomNumber, room.RoomNumber);
        var rooms = await Rooms();
        var existing = Existing(rooms, roomNumber);
        await store.Save([.. rooms.Select(stored => stored == existing ? room : stored)]);
        return room;
    }

    public async Task Delete(string roomNumber)
    {
        var rooms = await Rooms();
        var existing = Existing(rooms, roomNumber);
        await store.Save([.. rooms.Where(stored => stored != existing)]);
    }

    private static Room Existing(IEnumerable<Room> rooms, string roomNumber) =>
        Find(rooms, roomNumber) ?? throw new RoomNotFound(roomNumber);

    private static Room? Find(IEnumerable<Room> rooms, string roomNumber) =>
        rooms.FirstOrDefault(room => room.RoomNumber == roomNumber);
}
