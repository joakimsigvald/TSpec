namespace MyHotel.Contract.Bookings;

/// <summary>
/// The booking number is assigned by the hotel, never by the caller.
/// </summary>
public record Booking(
    int BookingNumber,
    BookingInfo Info,
    BookingPeriod Period,
    BookingState State,
    BookingPayment Payment);
