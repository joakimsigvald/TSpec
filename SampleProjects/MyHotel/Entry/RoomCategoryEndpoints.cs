using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MyHotel.Contract.Rooms;

namespace MyHotel.Entry;

public static class RoomCategoryEndpoints
{
    public static void MapRoomCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/room-categories", RoomCategories);
        app.MapPost("/room-categories", Add);
        app.MapPut("/room-categories/{name}", Update);
        app.MapDelete("/room-categories/{name}", Delete);
    }

    private static async Task<IResult> RoomCategories(IRoomCategoryService roomCategories) =>
        Results.Ok(await roomCategories.RoomCategories());

    /// <summary>No endpoint serves one category alone, so the reply points nowhere.</summary>
    private static async Task<IResult> Add(IRoomCategoryService roomCategories, RoomCategory roomCategory) =>
        Results.Created((string?)null, await roomCategories.Add(roomCategory));

    private static async Task<IResult> Update(
        IRoomCategoryService roomCategories, string name, RoomCategory roomCategory) =>
        Results.Ok(await roomCategories.Update(name, roomCategory));

    private static async Task<IResult> Delete(IRoomCategoryService roomCategories, string name)
    {
        await roomCategories.Delete(name);
        return Results.NoContent();
    }
}
