namespace MyHotel.Contract.Bookings;

/// <summary>
/// What a booking is today and what can be done with it, which the hotel works out and a caller never
/// sends. Two states are equal when their actions are, since a record compares a list by reference.
/// </summary>
public record BookingState(BookingStatus Status, IReadOnlyList<BookingAction> Actions)
{
    public virtual bool Equals(BookingState? other) =>
        other is not null && Status == other.Status && Actions.SequenceEqual(other.Actions);

    public override int GetHashCode() => HashCode.Combine(Status, Actions.Count);
}
