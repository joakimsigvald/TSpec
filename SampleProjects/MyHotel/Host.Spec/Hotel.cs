namespace MyHotel.Host.Spec;

/// <summary>
/// The application together with the rooms, room categories and bookings it keeps and the clock it
/// reads, so a specification can restart it or move today. The files outlive each application and
/// are deleted with the hotel.
/// </summary>
public sealed class Hotel : IDisposable
{
    private readonly string _roomsPath = TestApi.TempPath("rooms");
    private readonly string _roomCategoriesPath = TestApi.TempPath("room-categories");
    private readonly string _bookingsPath = TestApi.TempPath("bookings");
    private readonly Clock _clock = new();
    private TestApi _api;

    public Hotel()
    {
        _api = NewApi();
        Api = _api.CreateClient();
    }

    public HttpClient Api { get; private set; }

    /// <summary>
    /// A new application over the same rooms, room categories and bookings. Nothing is carried over
    /// in memory, so whatever answers afterwards was read back from storage.
    /// </summary>
    public void Restart()
    {
        Stop();
        _api = NewApi();
        Api = _api.CreateClient();
    }

    public void SetToday(DateOnly today) => _clock.Today = today;

    public void Dispose()
    {
        Stop();
        File.Delete(_roomsPath);
        File.Delete(_roomCategoriesPath);
        File.Delete(_bookingsPath);
    }

    private TestApi NewApi() => new(_clock, _roomsPath, _roomCategoriesPath, _bookingsPath);

    private void Stop()
    {
        Api.Dispose();
        _api.Dispose();
    }
}
