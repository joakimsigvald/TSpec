using System.Collections.Concurrent;
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

var rooms = new ConcurrentDictionary<string, Room>();

app.MapPost("/rooms", (Room room) => rooms.TryAdd(room.RoomNumber, room)
    ? Results.Created($"/rooms/{room.RoomNumber}", room)
    : Results.Conflict());

app.MapGet("/rooms/{roomNumber}", (string roomNumber) => rooms.TryGetValue(roomNumber, out var room)
    ? Results.Ok(room)
    : Results.NotFound());

app.Run();

public record VersionInfo(string Version);

public record Room(string RoomNumber, int BedCount);

public partial class Program;
