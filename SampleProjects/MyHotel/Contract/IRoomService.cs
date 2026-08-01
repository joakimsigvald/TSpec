namespace MyHotel.Contract;

/// <summary>
/// One method per endpoint, taking what the endpoint takes. Anything that would have been an early
/// return is thrown instead, so a method either returns the value the endpoint needs or does not
/// return at all.
/// </summary>
public interface IRoomService
{
    Task<IReadOnlyList<Room>> List();

    Task<Room> Get(string roomNumber);

    Task<Room> Add(Room room);

    Task<Room> Update(string roomNumber, Room room);

    Task Delete(string roomNumber);
}
