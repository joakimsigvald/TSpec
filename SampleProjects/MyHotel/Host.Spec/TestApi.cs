using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace MyHotel.Host.Spec;

/// <summary>
/// The API over given data files, reading today from the given clock. The files outlive the
/// application, so a shared path would let one test's data be visible to the next.
/// </summary>
/// <param name="roomsPath">
/// Where to keep the rooms. Omitted, the application gets a temporary file of its own and deletes
/// it on disposal; supplied, the caller owns the file, so two applications in succession see the
/// same rooms. The other paths work the same way.
/// </param>
internal sealed class TestApi(
    TimeProvider clock,
    string? roomsPath = null,
    string? roomCategoriesPath = null,
    string? bookingsPath = null)
    : WebApplicationFactory<Program>
{
    private readonly string _roomsPath = roomsPath ?? TempPath("rooms");
    private readonly string _roomCategoriesPath = roomCategoriesPath ?? TempPath("room-categories");
    private readonly string _bookingsPath = bookingsPath ?? TempPath("bookings");

    private readonly bool _ownsFiles =
        roomsPath is null && roomCategoriesPath is null && bookingsPath is null;

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder
            .UseSetting("RoomStore:Path", _roomsPath)
            .UseSetting("RoomCategoryStore:Path", _roomCategoriesPath)
            .UseSetting("BookingStore:Path", _bookingsPath)
            .ConfigureTestServices(services => services.AddSingleton(clock));

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing || !_ownsFiles)
            return;

        File.Delete(_roomsPath);
        File.Delete(_roomCategoriesPath);
        File.Delete(_bookingsPath);
    }

    internal static string TempPath(string kind) =>
        Path.Combine(Path.GetTempPath(), $"myhotel-{kind}-{Guid.NewGuid():N}.json");
}
