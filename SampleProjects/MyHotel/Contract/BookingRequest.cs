namespace MyHotel.Contract;

/// <summary>
/// What a caller books with: a <see cref="Booking"/> minus the id, which the hotel assigns.
/// </summary>
public record BookingRequest(string RoomNumber, string GuestName, DateOnly From, DateOnly To);
