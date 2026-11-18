using MyHotel.Contract.Bookings;
using MyHotel.Contract.Bookings.Exceptions;
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
        if (request.To <= request.From)
            throw new InvalidBookingPeriod(request.From, request.To);
        if (!await RoomExists(request.RoomNumber))
            throw new RoomNotFound(request.RoomNumber);
        var bookings = await store.Load();
        if (bookings.Any(booking => Overlaps(booking, request)))
            throw new RoomAlreadyBooked(request.RoomNumber);
        var booked = new StoredBooking(
            await numbers.Next(), request.RoomNumber, request.GuestName, request.From, request.To, false);
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

    private async Task<bool> RoomExists(string roomNumber) =>
        (await rooms.Load()).Any(room => room.RoomNumber == roomNumber);

    private static StoredBooking Existing(IEnumerable<StoredBooking> bookings, int bookingNumber) =>
        bookings.FirstOrDefault(booking => booking.BookingNumber == bookingNumber)
        ?? throw new BookingNotFound(bookingNumber);

    private static bool Overlaps(StoredBooking booking, BookingRequest request) =>
        !booking.Cancelled
        && booking.RoomNumber == request.RoomNumber
        && request.From < booking.To
        && booking.From < request.To;
}
