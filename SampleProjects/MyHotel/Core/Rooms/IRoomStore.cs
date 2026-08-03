using MyHotel.Contract;

namespace MyHotel.Core.Rooms;

/// <summary>
/// Where rooms are kept. Declared here and implemented outward, so Core owns the shape of its own
/// storage. Whole-list load and save: the rules live in Core, and a store that knew how to find or
/// replace one room would be holding some of them.
/// </summary>
public interface IRoomStore
{
    Task<IReadOnlyList<Room>> Load();

    Task Save(IReadOnlyList<Room> rooms);
}
