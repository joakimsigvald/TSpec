using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MyHotel.Contract;

namespace MyHotel.Entry;

public static class RoomEndpoints
{
    public static void MapRoomEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/rooms", async (IRoomService rooms) =>
        {
            var all = await rooms.List();
            return Results.Ok(all);
        });

        app.MapPost("/rooms", async (IRoomService rooms, Room room) =>
        {
            var added = await rooms.Add(room);
            return Results.Created($"/rooms/{added.RoomNumber}", added);
        });

        app.MapGet("/rooms/{roomNumber}", async (IRoomService rooms, string roomNumber) =>
        {
            var room = await rooms.Get(roomNumber);
            return Results.Ok(room);
        });

        app.MapPut("/rooms/{roomNumber}", async (IRoomService rooms, string roomNumber, Room room) =>
        {
            var updated = await rooms.Update(roomNumber, room);
            return Results.Ok(updated);
        });

        app.MapDelete("/rooms/{roomNumber}", async (IRoomService rooms, string roomNumber) =>
        {
            await rooms.Delete(roomNumber);
            return Results.NoContent();
        });
    }
}
