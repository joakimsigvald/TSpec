namespace MyHotel.Contract.Bookings;

/// <summary>
/// The booking number is assigned by the hotel, never by the caller.
/// </summary>
public record Booking(
    int BookingNumber,
    string RoomNumber,
    string GuestName,
    int GuestCount,
    BookingPeriod Period,
    BookingStatus Status);
