using MyHotel.Contract;
using MyHotel.Core.Rooms;

namespace MyHotel.Core.Bookings;

/// <summary>
/// Bookings are kept in the order they were made, which is the order the store returns them in.
/// Nights are half-open, [From, To): two bookings conflict only when their intervals truly
/// intersect, so one guest departing the day another arrives is no conflict.
/// </summary>
public class BookingService(IBookingStore store, IRoomStore rooms, IBookingNumberGenerator numbers)
    : IBookingService
{
    public async Task<IReadOnlyList<Booking>> List() => await store.Load();

    public async Task<Booking> Get(int bookingNumber) => Existing(await store.Load(), bookingNumber);

    public async Task<Booking> Book(BookingRequest request)
    {
        if (request.To <= request.From)
            throw new InvalidBookingPeriod(request.From, request.To);
        if (!await RoomExists(request.RoomNumber))
            throw new RoomNotFound(request.RoomNumber);
        var bookings = await Bookings();
        if (bookings.Any(booking => Overlaps(booking, request)))
            throw new RoomAlreadyBooked(request.RoomNumber);
        var booked = new Booking(
            await numbers.Next(), request.RoomNumber, request.GuestName, request.From, request.To);
        bookings.Add(booked);
        await store.Save(bookings);
        return booked;
    }

    public async Task Cancel(int bookingNumber)
    {
        var bookings = await Bookings();
        bookings.Remove(Existing(bookings, bookingNumber));
        await store.Save(bookings);
    }

    private async Task<bool> RoomExists(string roomNumber)
        => (await rooms.Load()).Any(room => room.RoomNumber == roomNumber);

    private async Task<List<Booking>> Bookings() => [.. await store.Load()];

    private static Booking Existing(IEnumerable<Booking> bookings, int bookingNumber)
        => bookings.FirstOrDefault(booking => booking.BookingNumber == bookingNumber)
        ?? throw new BookingNotFound(bookingNumber);

    private static bool Overlaps(Booking booking, BookingRequest request)
        => booking.RoomNumber == request.RoomNumber
        && request.From < booking.To
        && booking.From < request.To;
}
