namespace MyHotel.Core.Rooms;

/// <summary>
/// What Rooms asks of the bookings, declared here so that Bookings answers it and the dependency
/// keeps running from Bookings to Rooms (Architecture.Spec, SubdomainDependencies).
/// </summary>
public interface IRoomBookings
{
    Task<IReadOnlyList<string>> HeldRooms();
    Task<IReadOnlyList<string>> RoomsWithCancellableBookings();
}
