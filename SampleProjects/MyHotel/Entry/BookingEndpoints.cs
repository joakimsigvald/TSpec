using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MyHotel.Contract;

namespace MyHotel.Entry;

public static class BookingEndpoints
{
    public static void MapBookingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/bookings", async (IBookingService bookings) =>
        {
            var all = await bookings.List();
            return Results.Ok(all);
        });

        app.MapPost("/bookings", async (IBookingService bookings, BookingRequest request) =>
        {
            var booked = await bookings.Book(request);
            return Results.Created($"/bookings/{booked.Id}", booked);
        });

        app.MapGet("/bookings/{id:int}", async (IBookingService bookings, int id) =>
        {
            var booking = await bookings.Get(id);
            return Results.Ok(booking);
        });

        app.MapDelete("/bookings/{id:int}", async (IBookingService bookings, int id) =>
        {
            await bookings.Cancel(id);
            return Results.NoContent();
        });
    }
}
