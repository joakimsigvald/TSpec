using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MyHotel.Spec;

/// <summary>
/// The API over a given room file. Rooms now outlive the application, so a shared path would let one
/// test's rooms be visible to the next.
/// </summary>
/// <param name="roomsPath">
/// Where to keep the rooms. Omitted, the application gets a temporary file of its own and deletes it
/// on disposal; supplied, the caller owns the file — which is what lets two applications in
/// succession see the same rooms.
/// </param>
internal sealed class TestApi(string? roomsPath = null) : WebApplicationFactory<Program>
{
    private readonly string _roomsPath = roomsPath ?? TempRoomsPath();
    private readonly bool _ownsFile = roomsPath is null;

    internal static string TempRoomsPath()
        => Path.Combine(Path.GetTempPath(), $"myhotel-{Guid.NewGuid():N}.json");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
        => builder.UseSetting("RoomStore:Path", _roomsPath);

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && _ownsFile)
            File.Delete(_roomsPath);
    }
}
