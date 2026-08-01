namespace MyHotel.Contract;

public class RoomNotFound(string roomNumber)
    : Exception($"There is no room {roomNumber}.")
{
    public string RoomNumber { get; } = roomNumber;
}
