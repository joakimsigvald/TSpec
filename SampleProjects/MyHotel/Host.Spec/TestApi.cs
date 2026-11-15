using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MyHotel.Spec;

/// <summary>
/// The API over given room and booking files. Both outlive the application, so a shared path would
/// let one test's data be visible to the next.
/// </summary>
/// <param name="roomsPath">
/// Where to keep the rooms. Omitted, the application gets a temporary file of its own and deletes
/// it on disposal; supplied, the caller owns the file — which is what lets two applications in
/// succession see the same rooms. <paramref name="bookingsPath"/> works the same way.
/// </param>
internal sealed class TestApi(string? roomsPath = null, string? bookingsPath = null)
    : WebApplicationFactory<Program>
{
    private readonly string _roomsPath = roomsPath ?? TempPath("rooms");
    private readonly string _bookingsPath = bookingsPath ?? TempPath("bookings");
    private readonly bool _ownsFiles = roomsPath is null && bookingsPath is null;

    internal static string TempPath(string kind)
        => Path.Combine(Path.GetTempPath(), $"myhotel-{kind}-{Guid.NewGuid():N}.json");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
        => builder
            .UseSetting("RoomStore:Path", _roomsPath)
            .UseSetting("BookingStore:Path", _bookingsPath);

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing || !_ownsFiles)
            return;

        File.Delete(_roomsPath);
        File.Delete(_bookingsPath);
    }
}
