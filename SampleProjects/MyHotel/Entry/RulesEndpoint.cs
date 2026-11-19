using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using MyHotel.Contract;
using MyHotel.Contract.Rooms;

namespace MyHotel.Entry;

public static class RulesEndpoint
{
    public static void MapRulesEndpoint(this IEndpointRouteBuilder app) =>
        app.MapGet("/rules", () => new Rules(BedCountLimits.Min, BedCountLimits.Max));
}
