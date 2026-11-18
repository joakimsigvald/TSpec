namespace MyHotel.Core.Rooms;

/// <summary>
/// What Rooms asks of the bookings, declared here so that Bookings answers it and the dependency
/// keeps running from Bookings to Rooms (Architecture.Spec, SubdomainDependencies).
/// </summary>
public interface IOpenBookings
{
    /// <summary>
    /// The rooms with a current or future booking that is not cancelled.
    /// </summary>
    Task<IReadOnlyList<string>> RoomNumbers();
}
