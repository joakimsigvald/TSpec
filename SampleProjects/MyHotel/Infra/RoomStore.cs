using System.Text.Json;
using MyHotel.Contract;
using MyHotel.Core.Rooms;

namespace MyHotel.Infra;

/// <summary>
/// Rooms as a JSON file, so they survive a restart. A missing file is an empty hotel rather than an
/// error: nothing has been stored yet, which is the same thing.
/// </summary>
public class RoomStore(string path) : IRoomStore
{
    public async Task<IReadOnlyList<Room>> Load()
    {
        if (!File.Exists(path))
            return [];
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<List<Room>>(stream) ?? [];
    }

    public async Task Save(IReadOnlyList<Room> rooms)
    {
        if (Path.GetDirectoryName(path) is { Length: > 0 } directory)
            Directory.CreateDirectory(directory);
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, rooms);
    }
}
