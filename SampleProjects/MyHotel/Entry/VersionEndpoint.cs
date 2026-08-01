using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using MyHotel.Contract;

namespace MyHotel.Entry;

public static class VersionEndpoint
{
    /// <summary>
    /// The version is passed in rather than read here: it identifies the deployed application,
    /// which is Host's assembly, not Entry's.
    /// </summary>
    public static void MapVersionEndpoint(this IEndpointRouteBuilder app, string version)
        => app.MapGet("/version", () => new VersionInfo(version));
}
