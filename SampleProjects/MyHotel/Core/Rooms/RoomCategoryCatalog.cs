using MyHotel.Contract.Rooms;

namespace MyHotel.Core.Rooms;

/// <summary>
/// The hotel's room categories: those stored, or Standard while none is. Since the last category
/// cannot be deleted, nothing stored only ever means a new hotel.
/// </summary>
public sealed class RoomCategoryCatalog(IRoomCategoryStore store)
{
    private static readonly RoomCategory Standard = new("Standard", "Standardrum");

    public async Task<IReadOnlyList<RoomCategory>> Load() =>
        await store.Load() is { Count: > 0 } stored ? stored : [Standard];

    public Task Save(IReadOnlyList<RoomCategory> roomCategories) => store.Save(roomCategories);
}
