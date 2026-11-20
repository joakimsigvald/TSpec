namespace MyHotel.Contract.Rooms.Exceptions;

public class RoomHeld(string roomNumber)
    : Exception($"Room {roomNumber} is still held by a booking, so it cannot be deleted.")
{
    public string RoomNumber { get; } = roomNumber;
}
