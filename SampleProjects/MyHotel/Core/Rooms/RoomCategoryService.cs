using MyHotel.Contract.Rooms;
using MyHotel.Contract.Rooms.Exceptions;

namespace MyHotel.Core.Rooms;

/// <summary>
/// Room categories are kept in the order they were added, which is the order the catalog returns
/// them in.
/// </summary>
public class RoomCategoryService(RoomCategoryCatalog catalog, IRoomStore rooms) : IRoomCategoryService
{
    public async Task<IReadOnlyList<RoomCategoryOverview>> RoomCategories()
    {
        var used = await UsedNames();
        return [.. (await catalog.Load()).Select(category => Overview(category, used))];
    }

    public async Task<RoomCategoryOverview> Add(RoomCategory roomCategory)
    {
        var categories = await catalog.Load();
        if (Find(categories, roomCategory.Name) is not null)
            throw new RoomCategoryAlreadyExists(roomCategory.Name);
        await catalog.Save([.. categories, roomCategory]);
        return await OverviewOf(roomCategory);
    }

    public async Task<RoomCategoryOverview> Update(string name, RoomCategory roomCategory)
    {
        if (roomCategory.Name != name)
            throw new RoomCategoryNameMismatch(name, roomCategory.Name);
        var categories = await catalog.Load();
        var existing = Existing(categories, name);
        await catalog.Save([.. categories.Select(stored => stored == existing ? roomCategory : stored)]);
        return await OverviewOf(roomCategory);
    }

    public async Task Delete(string name)
    {
        var categories = await catalog.Load();
        var existing = Existing(categories, name);
        if (categories.Count == 1)
            throw new RoomCategoryIsLast(name);
        if ((await UsedNames()).Contains(name))
            throw new RoomCategoryHasRooms(name);

        await catalog.Save([.. categories.Where(stored => stored != existing)]);
    }

    private async Task<RoomCategoryOverview> OverviewOf(RoomCategory roomCategory) =>
        Overview(roomCategory, await UsedNames());

    private async Task<IReadOnlySet<string>> UsedNames()
    {
        HashSet<string> used = [.. (await rooms.Load()).Select(room => room.RoomCategory)];
        return used;
    }

    private static RoomCategoryOverview Overview(RoomCategory roomCategory, IReadOnlySet<string> used) =>
        new(roomCategory.Name, roomCategory.Description, used.Contains(roomCategory.Name));

    private static RoomCategory Existing(IEnumerable<RoomCategory> categories, string name) =>
        Find(categories, name) ?? throw new RoomCategoryNotFound(name);

    private static RoomCategory? Find(IEnumerable<RoomCategory> categories, string name) =>
        categories.FirstOrDefault(category => category.Name == name);
}
