using MyHotel.Contract.Rooms;
using MyHotel.Contract.Rooms.Exceptions;

namespace MyHotel.Core.Rooms;

/// <summary>
/// Rooms are kept in the order they were created, which is the order the store returns them in.
/// </summary>
public class RoomService(
    IRoomStore store,
    IOpenBookings openBookings,
    RoomCategoryCatalog roomCategories)
    : IRoomService
{
    public async Task<IReadOnlyList<RoomOverview>> Rooms()
    {
        var open = await openBookings.RoomNumbers();
        return [.. (await store.Load()).Select(room => Overview(room, open))];
    }

    public async Task<RoomOverview> RoomFor(string roomNumber) =>
        await OverviewOf(Existing(await store.Load(), roomNumber));

    public async Task<RoomOverview> Add(Room room)
    {
        var rooms = await store.Load();
        if (Find(rooms, room.RoomNumber) is not null)
            throw new RoomAlreadyExists(room.RoomNumber);
        if (!await RoomCategoryExists(room.RoomCategory))
            throw new RoomCategoryNotFound(room.RoomCategory);
        await store.Save([.. rooms, room]);
        return await OverviewOf(room);
    }

    public async Task<RoomOverview> Update(string roomNumber, Room room)
    {
        if (room.RoomNumber != roomNumber)
            throw new RoomNumberMismatch(roomNumber, room.RoomNumber);
        var rooms = await store.Load();
        var existing = Existing(rooms, roomNumber);
        if (!await RoomCategoryExists(room.RoomCategory))
            throw new RoomCategoryNotFound(room.RoomCategory);
        await store.Save([.. rooms.Select(stored => stored == existing ? room : stored)]);
        return await OverviewOf(room);
    }

    public async Task Delete(string roomNumber)
    {
        var rooms = await store.Load();
        var existing = Existing(rooms, roomNumber);
        if ((await openBookings.RoomNumbers()).Contains(roomNumber))
            throw new RoomHasOpenBookings(roomNumber);

        await store.Save([.. rooms.Where(stored => stored != existing)]);
    }

    private async Task<bool> RoomCategoryExists(string name) =>
        (await roomCategories.Load()).Any(category => category.Name == name);

    private async Task<RoomOverview> OverviewOf(Room room) =>
        Overview(room, await openBookings.RoomNumbers());

    private static RoomOverview Overview(Room room, IReadOnlyList<string> open) =>
        new(room.RoomNumber, room.BedCount, room.RoomCategory, open.Contains(room.RoomNumber));

    private static Room Existing(IEnumerable<Room> rooms, string roomNumber) =>
        Find(rooms, roomNumber) ?? throw new RoomNotFound(roomNumber);

    private static Room? Find(IEnumerable<Room> rooms, string roomNumber) =>
        rooms.FirstOrDefault(room => room.RoomNumber == roomNumber);
}
