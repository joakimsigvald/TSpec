namespace MyHotel.Contract;

/// <summary>
/// Nights are half-open, <c>[From, To)</c>: the guest departs on <c>To</c>, so that night is free
/// for the next booking. The booking number is assigned by the hotel, never by the caller.
/// </summary>
public record Booking(int BookingNumber, string RoomNumber, string GuestName, DateOnly From, DateOnly To);
