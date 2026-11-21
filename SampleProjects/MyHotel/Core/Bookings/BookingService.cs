using MyHotel.Contract.Bookings;
using MyHotel.Contract.Bookings.Exceptions;
using MyHotel.Contract.Rooms;
using MyHotel.Contract.Rooms.Exceptions;
using MyHotel.Core.Rooms;

namespace MyHotel.Core.Bookings;

/// <summary>
/// Bookings are kept in the order they were made, which is the order the store returns them in.
/// Nights are half-open, [From, To): two bookings conflict only when their intervals truly
/// intersect, so one guest departing the day another arrives is no conflict.
/// </summary>
public class BookingService(
    IBookingStore store,
    IRoomStore rooms,
    IBookingNumberGenerator numbers,
    TimeProvider clock)
    : IBookingService
{
    public async Task<IReadOnlyList<Booking>> Bookings() => [.. (await store.Load()).Select(Sent)];

    public async Task<Booking> BookingFor(int bookingNumber) =>
        Sent(Existing(await store.Load(), bookingNumber));

    public async Task<Booking> Book(BookingRequest request)
    {
        var room = await BookableRoomFor(request);
        var bookings = await store.Load();
        EnsureRoomFree(request, bookings);
        var booked = new StoredBooking(
            await numbers.Next(),
            request.Info,
            request.Period,
            room.PricePerNight,
            BookingStage.Booked,
            Paid: false);
        await store.Save([.. bookings, booked]);
        return Sent(booked);
    }

    public async Task<Booking> Change(int bookingNumber, BookingRequest request)
    {
        var room = await BookableRoomFor(request);
        var bookings = await store.Load();
        var existing = Existing(bookings, bookingNumber);
        if (existing.Stage != BookingStage.Cancelled)
            EnsureRoomFree(request, bookings.Where(booking => booking != existing));
        var changed = existing.ChangedTo(request, await PricePerNightIn(room, existing));
        await store.Save([.. bookings.Select(booking => booking == existing ? changed : booking)]);
        return Sent(changed);
    }

    public Task Cancel(int bookingNumber) => Take(BookingAction.Cancel, bookingNumber);
    public Task CheckIn(int bookingNumber) => Take(BookingAction.CheckIn, bookingNumber);
    public Task CheckOut(int bookingNumber) => Take(BookingAction.CheckOut, bookingNumber);
    public Task Pay(int bookingNumber) => Take(BookingAction.Pay, bookingNumber);

    public async Task CancelBookings(string roomNumber)
    {
        var today = clock.Today();
        await SaveTaking(
            BookingAction.Cancel,
            await store.Load(),
            booking => booking.Info.RoomNumber == roomNumber && booking.IsCancellableOn(today));
    }

    private async Task Take(BookingAction action, int bookingNumber)
    {
        var bookings = await store.Load();
        var taken = Existing(bookings, bookingNumber);
        if (!taken.Allows(action, clock.Today()))
            throw Refusal(action, bookingNumber);

        await SaveTaking(action, bookings, booking => booking == taken);
    }

    private async Task<Room> BookableRoomFor(BookingRequest request)
    {
        if (request.Period.To <= request.Period.From)
            throw new InvalidBookingPeriod(request.Period.From, request.Period.To);
        if (request.Info.GuestCount < 1)
            throw new InvalidGuestCount(request.Info.GuestCount);
        var room = await RoomFor(request.Info.RoomNumber);
        return request.Info.GuestCount > room.BedCount
            ? throw new TooManyGuests(request.Info.RoomNumber, request.Info.GuestCount)
            : room;
    }

    /// <summary>
    /// A booking moved within its room's category keeps its price per night. The categories compared are
    /// the rooms' today, and a room no longer there counts as another category.
    /// </summary>
    private async Task<int> PricePerNightIn(Room room, StoredBooking booking) =>
        (await Find(booking.Info.RoomNumber))?.RoomCategory == room.RoomCategory
            ? booking.PricePerNight
            : room.PricePerNight;

    private Booking Sent(StoredBooking booking) => booking.AsOf(clock.Today());

    private Task SaveTaking(
        BookingAction action, IReadOnlyList<StoredBooking> bookings, Func<StoredBooking, bool> takes) =>
        store.Save([.. bookings.Select(booking => takes(booking) ? booking.After(action) : booking)]);

    private async Task<Room> RoomFor(string roomNumber) =>
        await Find(roomNumber) ?? throw new RoomNotFound(roomNumber);

    private async Task<Room?> Find(string roomNumber) =>
        (await rooms.Load()).FirstOrDefault(room => room.RoomNumber == roomNumber);

    private static StoredBooking Existing(IEnumerable<StoredBooking> bookings, int bookingNumber) =>
        bookings.FirstOrDefault(booking => booking.BookingNumber == bookingNumber)
        ?? throw new BookingNotFound(bookingNumber);

    private static void EnsureRoomFree(BookingRequest request, IEnumerable<StoredBooking> others)
    {
        if (others.Any(booking => Overlaps(booking, request)))
            throw new RoomAlreadyBooked(request.Info.RoomNumber);
    }

    private static Exception Refusal(BookingAction action, int bookingNumber) =>
        action switch
        {
            BookingAction.Cancel => new BookingNotCancellable(bookingNumber),
            BookingAction.CheckIn => new NotDueForCheckIn(bookingNumber),
            BookingAction.CheckOut => new NotDueForCheckOut(bookingNumber),
            _ => new BookingNotPayable(bookingNumber),
        };

    private static bool Overlaps(StoredBooking booking, BookingRequest request) =>
        booking.Stage != BookingStage.Cancelled
        && booking.Info.RoomNumber == request.Info.RoomNumber
        && request.Period.From < booking.Period.To
        && booking.Period.From < request.Period.To;
}
