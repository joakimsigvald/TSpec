namespace MyHotel.Contract;

public class RoomAlreadyExists(string roomNumber)
    : Exception($"Room {roomNumber} already exists.")
{
    public string RoomNumber { get; } = roomNumber;
}
