namespace MyHotel.Contract.Rooms;

/// <summary>
/// One method per endpoint, as <see cref="IRoomService"/>, and for the same reason.
/// </summary>
public interface IRoomCategoryService
{
    Task<IReadOnlyList<RoomCategoryOverview>> RoomCategories();
    Task<RoomCategoryOverview> Add(RoomCategory roomCategory);
    Task<RoomCategoryOverview> Update(string name, RoomCategory roomCategory);
    Task Delete(string name);
}
