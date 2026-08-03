namespace MyHotel.Contract;

public class InvalidBookingPeriod(DateOnly from, DateOnly to)
    : Exception($"A booking must last at least one night: from {from:o} to {to:o} does not.")
{
    public DateOnly From { get; } = from;
    public DateOnly To { get; } = to;
}
