using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MyHotel.Contract.Rooms;

namespace MyHotel.Entry;

public static class RoomEndpoints
{
    public static void MapRoomEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/rooms", Rooms);
        app.MapPost("/rooms", Add);
        app.MapGet("/rooms/{roomNumber}", RoomFor);
        app.MapPut("/rooms/{roomNumber}", Update);
        app.MapDelete("/rooms/{roomNumber}", Delete);
    }

    private static async Task<IResult> Rooms(IRoomService rooms) => Results.Ok(await rooms.Rooms());

    private static async Task<IResult> Add(IRoomService rooms, Room room)
    {
        var added = await rooms.Add(room);
        return Results.Created($"/rooms/{added.RoomNumber}", added);
    }

    private static async Task<IResult> RoomFor(IRoomService rooms, string roomNumber) =>
        Results.Ok(await rooms.RoomFor(roomNumber));

    private static async Task<IResult> Update(IRoomService rooms, string roomNumber, Room room) =>
        Results.Ok(await rooms.Update(roomNumber, room));

    private static async Task<IResult> Delete(IRoomService rooms, string roomNumber)
    {
        await rooms.Delete(roomNumber);
        return Results.NoContent();
    }
}
