using System.Net.Http.Json;

namespace MyHotel.Spec;

internal static class ResponseExtensions
{
    internal static async Task<T> Read<T>(this HttpResponseMessage response)
        => (await response.Content.ReadFromJsonAsync<T>(TestContext.Current.CancellationToken))!;
}
