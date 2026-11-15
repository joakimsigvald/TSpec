namespace MyHotel.Contract.Rooms.Exceptions;

public class RoomAlreadyExists(string roomNumber)
    : Exception($"Room {roomNumber} already exists.")
{
    public string RoomNumber { get; } = roomNumber;
}
