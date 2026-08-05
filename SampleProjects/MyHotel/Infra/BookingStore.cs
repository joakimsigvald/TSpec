using System.Text.Json;
using MyHotel.Contract;
using MyHotel.Core.Bookings;

namespace MyHotel.Infra;

/// <summary>
/// The booking book as a JSON file, so bookings and their numbering both survive a restart. A
/// missing file is an empty book rather than an error: nothing has been stored yet, which is the
/// same thing.
/// </summary>
/// <remarks>
/// One file and one class for both, because the number to issue next is a fact about this hotel's
/// bookings and nothing else reads it. Each save reads the book first and writes it whole, so
/// storing bookings does not forget the numbering, or the other way round.
/// </remarks>
public class BookingStore(string path) : IBookingStore, IBookingNumberStore
{
    public async Task<IReadOnlyList<Booking>> Load() => (await Read()).Bookings;

    public async Task Save(IReadOnlyList<Booking> bookings)
        => await Write(await Read() with { Bookings = [.. bookings] });

    public async Task<int?> LoadLastUsed() => (await Read()).LastUsedNumber;

    public async Task SaveLastUsed(int number)
        => await Write(await Read() with { LastUsedNumber = number });

    private async Task<Book> Read()
    {
        if (!File.Exists(path))
            return Book.Empty;

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<Book>(stream) ?? Book.Empty;
    }

    private async Task Write(Book book)
    {
        if (Path.GetDirectoryName(path) is { Length: > 0 } directory)
            Directory.CreateDirectory(directory);
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, book);
    }

    /// What the file holds: the bookings made, and the last number issued — null until the first
    /// one is, which is the one moment the seed applies.
    private record Book(List<Booking> Bookings, int? LastUsedNumber)
    {
        internal static Book Empty => new([], null);
    }
}
