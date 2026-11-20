namespace MyHotel.Core.Spec;

/// <summary>
/// What the store is handed for a booking that was cancelled, checked in or checked out. A
/// verification states it through these, since an expression tree cannot hold a with-expression.
/// </summary>
internal static class Marking
{
    internal static StoredBooking MarkedCancelled(StoredBooking booking) =>
        booking with { Stage = BookingStage.Cancelled };

    internal static StoredBooking MarkedCheckedIn(StoredBooking booking) =>
        booking with { Stage = BookingStage.CheckedIn };

    internal static StoredBooking MarkedCheckedOut(StoredBooking booking) =>
        booking with { Stage = BookingStage.CheckedOut };
}
