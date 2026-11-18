using MyHotel.Contract.Rooms;

namespace MyHotel.Core.Rooms;

/// <summary>
/// Where room categories are kept: whole-list load and save, like <see cref="IRoomStore"/> and for
/// the same reason.
/// </summary>
public interface IRoomCategoryStore
{
    Task<IReadOnlyList<RoomCategory>> Load();
    Task Save(IReadOnlyList<RoomCategory> roomCategories);
}
