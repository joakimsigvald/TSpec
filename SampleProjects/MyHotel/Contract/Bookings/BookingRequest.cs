namespace MyHotel.Contract.Bookings;

/// <summary>
/// What a caller books with: a <see cref="Booking"/> minus the booking number, which the hotel assigns.
/// </summary>
public record BookingRequest(string RoomNumber, string GuestName, int GuestCount, BookingPeriod Period);
