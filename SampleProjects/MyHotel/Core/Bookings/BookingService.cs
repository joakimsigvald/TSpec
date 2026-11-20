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
        await EnsureBookable(request);
        var bookings = await store.Load();
        EnsureRoomFree(request, bookings);
        var booked = new StoredBooking(
            await numbers.Next(),
            request.RoomNumber,
            request.GuestName,
            request.GuestCount,
            request.Period,
            BookingStage.Booked);
        await store.Save([.. bookings, booked]);
        return Sent(booked);
    }

    public async Task<Booking> Change(int bookingNumber, BookingRequest request)
    {
        await EnsureBookable(request);
        var bookings = await store.Load();
        var existing = Existing(bookings, bookingNumber);
        if (existing.Stage != BookingStage.Cancelled)
            EnsureRoomFree(request, bookings.Where(booking => booking != existing));
        var changed = existing.ChangedTo(request);
        await store.Save([.. bookings.Select(booking => booking == existing ? changed : booking)]);
        return Sent(changed);
    }

    public Task Cancel(int bookingNumber) => Take(BookingAction.Cancel, bookingNumber);
    public Task CheckIn(int bookingNumber) => Take(BookingAction.CheckIn, bookingNumber);
    public Task CheckOut(int bookingNumber) => Take(BookingAction.CheckOut, bookingNumber);

    public async Task CancelBookings(string roomNumber)
    {
        var today = clock.Today();
        await SaveTaking(
            BookingAction.Cancel,
            await store.Load(),
            booking => booking.RoomNumber == roomNumber && booking.IsCancellableOn(today));
    }

    private async Task Take(BookingAction action, int bookingNumber)
    {
        var bookings = await store.Load();
        var taken = Existing(bookings, bookingNumber);
        if (!taken.Allows(action, clock.Today()))
            throw Refusal(action, bookingNumber);

        await SaveTaking(action, bookings, booking => booking == taken);
    }

    private async Task EnsureBookable(BookingRequest request)
    {
        if (request.Period.To <= request.Period.From)
            throw new InvalidBookingPeriod(request.Period.From, request.Period.To);
        if (request.GuestCount < 1)
            throw new InvalidGuestCount(request.GuestCount);
        var room = await RoomFor(request.RoomNumber);
        if (request.GuestCount > room.BedCount)
            throw new TooManyGuests(request.RoomNumber, request.GuestCount);
    }

    private Booking Sent(StoredBooking booking) => booking.AsOf(clock.Today());

    private Task SaveTaking(
        BookingAction action, IReadOnlyList<StoredBooking> bookings, Func<StoredBooking, bool> takes) =>
        store.Save([.. bookings.Select(booking => takes(booking) ? booking.After(action) : booking)]);

    private async Task<Room> RoomFor(string roomNumber) =>
        (await rooms.Load()).FirstOrDefault(room => room.RoomNumber == roomNumber)
        ?? throw new RoomNotFound(roomNumber);

    private static StoredBooking Existing(IEnumerable<StoredBooking> bookings, int bookingNumber) =>
        bookings.FirstOrDefault(booking => booking.BookingNumber == bookingNumber)
        ?? throw new BookingNotFound(bookingNumber);

    private static void EnsureRoomFree(BookingRequest request, IEnumerable<StoredBooking> others)
    {
        if (others.Any(booking => Overlaps(booking, request)))
            throw new RoomAlreadyBooked(request.RoomNumber);
    }

    private static Exception Refusal(BookingAction action, int bookingNumber) =>
        action switch
        {
            BookingAction.Cancel => new BookingNotCancellable(bookingNumber),
            BookingAction.CheckIn => new NotDueForCheckIn(bookingNumber),
            _ => new NotDueForCheckOut(bookingNumber),
        };

    private static bool Overlaps(StoredBooking booking, BookingRequest request) =>
        booking.Stage != BookingStage.Cancelled
        && booking.RoomNumber == request.RoomNumber
        && request.Period.From < booking.Period.To
        && booking.Period.From < request.Period.To;
}
