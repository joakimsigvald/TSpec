namespace MyHotel.Contract.Rooms.Exceptions;

public class RoomHasOpenBookings(string roomNumber)
    : Exception($"Room {roomNumber} has current or future bookings, so it cannot be deleted.")
{
    public string RoomNumber { get; } = roomNumber;
}
