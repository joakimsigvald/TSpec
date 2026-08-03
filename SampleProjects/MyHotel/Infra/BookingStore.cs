using System.Text.Json;
using MyHotel.Contract;
using MyHotel.Core.Bookings;

namespace MyHotel.Infra;

/// <summary>
/// Bookings as a JSON file, so they survive a restart. A missing file is an empty book rather than
/// an error: nothing has been stored yet, which is the same thing.
/// </summary>
public class BookingStore(string path) : IBookingStore
{
    public async Task<IReadOnlyList<Booking>> Load()
    {
        if (!File.Exists(path))
            return [];
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<List<Booking>>(stream) ?? [];
    }

    public async Task Save(IReadOnlyList<Booking> bookings)
    {
        if (Path.GetDirectoryName(path) is { Length: > 0 } directory)
            Directory.CreateDirectory(directory);
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, bookings);
    }
}
