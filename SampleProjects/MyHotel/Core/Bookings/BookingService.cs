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
        if (request.Period.To <= request.Period.From)
            throw new InvalidBookingPeriod(request.Period.From, request.Period.To);
        if (request.GuestCount < 1)
            throw new InvalidGuestCount(request.GuestCount);
        var room = await RoomFor(request.RoomNumber);
        if (request.GuestCount > room.BedCount)
            throw new TooManyGuests(request.RoomNumber, request.GuestCount);
        var bookings = await store.Load();
        if (bookings.Any(booking => Overlaps(booking, request)))
            throw new RoomAlreadyBooked(request.RoomNumber);
        var booked = new StoredBooking(
            await numbers.Next(),
            request.RoomNumber,
            request.GuestName,
            request.GuestCount,
            request.Period,
            false);
        await store.Save([.. bookings, booked]);
        return Sent(booked);
    }

    public async Task Cancel(int bookingNumber)
    {
        var bookings = await store.Load();
        var cancelled = Existing(bookings, bookingNumber);
        if (!cancelled.IsOpenOn(clock.Today()))
            throw new BookingNotOpen(bookingNumber);

        await SaveCancelling(bookings, booking => booking == cancelled);
    }

    public async Task CancelOpenBookings(string roomNumber)
    {
        var today = clock.Today();
        await SaveCancelling(
            await store.Load(),
            booking => booking.RoomNumber == roomNumber && booking.IsOpenOn(today));
    }

    private Booking Sent(StoredBooking booking) => booking.AsOf(clock.Today());

    private Task SaveCancelling(IReadOnlyList<StoredBooking> bookings, Func<StoredBooking, bool> cancels) =>
        store.Save([.. bookings.Select(booking => cancels(booking) ? booking.AsCancelled() : booking)]);

    private async Task<Room> RoomFor(string roomNumber) =>
        (await rooms.Load()).FirstOrDefault(room => room.RoomNumber == roomNumber)
        ?? throw new RoomNotFound(roomNumber);

    private static StoredBooking Existing(IEnumerable<StoredBooking> bookings, int bookingNumber) =>
        bookings.FirstOrDefault(booking => booking.BookingNumber == bookingNumber)
        ?? throw new BookingNotFound(bookingNumber);

    private static bool Overlaps(StoredBooking booking, BookingRequest request) =>
        !booking.Cancelled
        && booking.RoomNumber == request.RoomNumber
        && request.Period.From < booking.Period.To
        && booking.Period.From < request.Period.To;
}
