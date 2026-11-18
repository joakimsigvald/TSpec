namespace MyHotel.Contract.Bookings;

/// <summary>
/// Nights are half-open, <c>[From, To)</c>: the guest departs on <c>To</c>, so that night is free
/// for the next booking.
/// </summary>
public record BookingPeriod(DateOnly From, DateOnly To);
