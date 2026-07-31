using System.Reflection;
using Scalar.AspNetCore;

var version = Assembly.GetExecutingAssembly()
    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
    .InformationalVersion.Split('+')[0];

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/version", () => new VersionInfo(version));

// A list, because rooms are listed in the order they were created.
var rooms = new List<Room>();

Room? Find(string roomNumber) => rooms.Find(room => room.RoomNumber == roomNumber);

app.MapGet("/rooms", () => rooms);

app.MapPost("/rooms", (Room room) =>
{
    if (Find(room.RoomNumber) is not null)
        return Results.Conflict();
    rooms.Add(room);
    return Results.Created($"/rooms/{room.RoomNumber}", room);
});

app.MapGet("/rooms/{roomNumber}", (string roomNumber)
    => Find(roomNumber) is { } room ? Results.Ok(room) : Results.NotFound());

app.MapPut("/rooms/{roomNumber}", (string roomNumber, Room room) =>
{
    if (Find(roomNumber) is not { } existing)
        return Results.NotFound();
    rooms[rooms.IndexOf(existing)] = room;
    return Results.Ok(room);
});

app.MapDelete("/rooms/{roomNumber}", (string roomNumber) =>
{
    if (Find(roomNumber) is not { } room)
        return Results.NotFound();
    rooms.Remove(room);
    return Results.NoContent();
});

app.Run();

public record VersionInfo(string Version);

public record Room(string RoomNumber, int BedCount);

public partial class Program;
