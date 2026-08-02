namespace MyHotel.Contract;

/// <summary>
/// The room number identifies the room, so an update cannot carry a different one. Renumbering a
/// room means deleting it and adding another.
/// </summary>
public class RoomNumberMismatch(string roomNumber, string given)
    : Exception($"Room {roomNumber} cannot be renumbered to {given}.")
{
    public string RoomNumber { get; } = roomNumber;

    public string Given { get; } = given;
}
