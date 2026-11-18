using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MyHotel.Contract.Bookings;

namespace MyHotel.Entry;

public static class BookingEndpoints
{
    public static void MapBookingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/bookings", Bookings);
        app.MapPost("/bookings", Book);
        app.MapGet("/bookings/{bookingNumber:int}", BookingFor);
        app.MapDelete("/bookings/{bookingNumber:int}", Cancel);
        app.MapDelete("/rooms/{roomNumber}/bookings", CancelOpenBookings);
    }

    private static async Task<IResult> Bookings(IBookingService bookings) =>
        Results.Ok(await bookings.Bookings());

    private static async Task<IResult> Book(IBookingService bookings, BookingRequest request)
    {
        var booked = await bookings.Book(request);
        return Results.Created($"/bookings/{booked.BookingNumber}", booked);
    }

    private static async Task<IResult> BookingFor(IBookingService bookings, int bookingNumber) =>
        Results.Ok(await bookings.BookingFor(bookingNumber));

    private static async Task<IResult> Cancel(IBookingService bookings, int bookingNumber)
    {
        await bookings.Cancel(bookingNumber);
        return Results.NoContent();
    }

    private static async Task<IResult> CancelOpenBookings(IBookingService bookings, string roomNumber)
    {
        await bookings.CancelOpenBookings(roomNumber);
        return Results.NoContent();
    }
}
