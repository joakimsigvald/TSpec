using System.Text.Json;
using MyHotel.Contract.Rooms;
using MyHotel.Core.Rooms;

namespace MyHotel.Infra;

/// <summary>
/// Room categories as a JSON file, so they survive a restart. A missing file is nothing stored
/// rather than an error, as for <see cref="RoomStore"/>.
/// </summary>
internal sealed class RoomCategoryStore(string path) : IRoomCategoryStore
{
    public async Task<IReadOnlyList<RoomCategory>> Load()
    {
        if (!File.Exists(path))
            return [];

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<List<RoomCategory>>(stream) ?? [];
    }

    public async Task Save(IReadOnlyList<RoomCategory> roomCategories)
    {
        if (Path.GetDirectoryName(path) is { Length: > 0 } directory)
            Directory.CreateDirectory(directory);
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, roomCategories);
    }
}
