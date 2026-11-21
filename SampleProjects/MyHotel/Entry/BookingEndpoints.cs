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
        app.MapPut("/bookings/{bookingNumber:int}", Change);
        app.MapDelete("/bookings/{bookingNumber:int}", Cancel);
        app.MapPost("/bookings/{bookingNumber:int}/check-in", CheckIn);
        app.MapPost("/bookings/{bookingNumber:int}/check-out", CheckOut);
        app.MapPost("/bookings/{bookingNumber:int}/pay", Pay);
        app.MapDelete("/rooms/{roomNumber}/bookings", CancelBookings);
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

    private static async Task<IResult> Change(
        IBookingService bookings, int bookingNumber, BookingRequest request) =>
        Results.Ok(await bookings.Change(bookingNumber, request));

    private static async Task<IResult> Cancel(IBookingService bookings, int bookingNumber)
    {
        await bookings.Cancel(bookingNumber);
        return Results.NoContent();
    }

    private static async Task<IResult> CheckIn(IBookingService bookings, int bookingNumber)
    {
        await bookings.CheckIn(bookingNumber);
        return Results.NoContent();
    }

    private static async Task<IResult> CheckOut(IBookingService bookings, int bookingNumber)
    {
        await bookings.CheckOut(bookingNumber);
        return Results.NoContent();
    }

    private static async Task<IResult> Pay(IBookingService bookings, int bookingNumber)
    {
        await bookings.Pay(bookingNumber);
        return Results.NoContent();
    }

    private static async Task<IResult> CancelBookings(IBookingService bookings, string roomNumber)
    {
        await bookings.CancelBookings(roomNumber);
        return Results.NoContent();
    }
}
