namespace MyHotel.Contract.Bookings.Exceptions;

public class RoomAlreadyBooked(string roomNumber)
    : Exception($"Room {roomNumber} is already booked for some of the requested nights.")
{
    public string RoomNumber { get; } = roomNumber;
}
