namespace MyHotel.Spec;

/// <summary>
/// The application together with the rooms it keeps, so a specification can restart it. The room
/// file outlives each application and is deleted with the hotel.
/// </summary>
public sealed class Hotel : IDisposable
{
    private readonly string _roomsPath = TestApi.TempRoomsPath();
    private TestApi _api;

    public Hotel()
    {
        _api = new TestApi(_roomsPath);
        Api = _api.CreateClient();
    }

    public HttpClient Api { get; private set; }

    /// <summary>
    /// A new application over the same rooms. Nothing is carried over in memory, so whatever answers
    /// afterwards was read back from storage.
    /// </summary>
    public void Restart()
    {
        Stop();
        _api = new TestApi(_roomsPath);
        Api = _api.CreateClient();
    }

    public void Dispose()
    {
        Stop();
        File.Delete(_roomsPath);
    }

    private void Stop()
    {
        Api.Dispose();
        _api.Dispose();
    }
}
