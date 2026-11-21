namespace MyHotel.Contract.Bookings;

/// <summary>
/// What a caller books with: a <see cref="Booking"/>'s info and period, without the booking number, the
/// state and the payment, which the hotel works out.
/// </summary>
public record BookingRequest(BookingInfo Info, BookingPeriod Period);
