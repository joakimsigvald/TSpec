namespace MyHotel.Core.Spec;

internal static class Cancelling
{
    /// <summary>
    /// What the store is handed for a cancelled booking. A verification states it through this,
    /// since an expression tree cannot hold a with-expression.
    /// </summary>
    internal static StoredBooking MarkedCancelled(StoredBooking booking) => booking with { Cancelled = true };
}
