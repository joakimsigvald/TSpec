namespace MyHotel.Spec;

/// <summary>
/// The application together with the rooms and bookings it keeps, so a specification can restart
/// it. The files outlive each application and are deleted with the hotel.
/// </summary>
public sealed class Hotel : IDisposable
{
    private readonly string _roomsPath = TestApi.TempPath("rooms");
    private readonly string _bookingsPath = TestApi.TempPath("bookings");
    private TestApi _api;

    public Hotel()
    {
        _api = new TestApi(_roomsPath, _bookingsPath);
        Api = _api.CreateClient();
    }

    public HttpClient Api { get; private set; }

    /// <summary>
    /// A new application over the same rooms and bookings. Nothing is carried over in memory, so
    /// whatever answers afterwards was read back from storage.
    /// </summary>
    public void Restart()
    {
        Stop();
        _api = new TestApi(_roomsPath, _bookingsPath);
        Api = _api.CreateClient();
    }

    public void Dispose()
    {
        Stop();
        File.Delete(_roomsPath);
        File.Delete(_bookingsPath);
    }

    private void Stop()
    {
        Api.Dispose();
        _api.Dispose();
    }
}
