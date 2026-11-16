namespace TSpec.Architecture.CSharp.Test;

/// <summary>
/// The projects of MyHotel's build as TSpec reads them, with its spec project referencing every project.
/// </summary>
internal static class MyHotelGraph
{
    public static SourceProjects Projects => new(new()
    {
        ["MyHotel"] = ["MyHotel.Entry", "MyHotel.Infra", "P:Scalar.AspNetCore"],
        ["MyHotel.Contract"] = [],
        ["MyHotel.Core"] = ["MyHotel.Contract", "P:Microsoft.Extensions.DependencyInjection.Abstractions"],
        ["MyHotel.Entry"] = ["MyHotel.Contract"],
        ["MyHotel.Infra"] = ["MyHotel.Core"],
        ["MyHotel.Spec"] = ["MyHotel", "MyHotel.Contract", "P:xunit.v3"],
    }, spec: "MyHotel.Spec");
}
