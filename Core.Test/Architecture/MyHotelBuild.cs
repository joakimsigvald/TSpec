namespace TSpec.Test.Architecture;

/// <summary>
/// The dependency manifest of MyHotel.Spec, cut down to what the graph reads: the layered projects,
/// a package on each side of them, and the spec project's own references.
/// </summary>
internal static class MyHotelBuild
{
    internal const string DepsJson =
        """
        {
          "runtimeTarget": { "name": ".NETCoreApp,Version=v10.0" },
          "targets": {
            ".NETCoreApp,Version=v10.0": {
              "MyHotel.Spec/1.0.0": {
                "dependencies": { "MyHotel": "0.2.0", "MyHotel.Contract": "0.1.0", "xunit.v3": "4.0.1" }
              },
              "MyHotel/0.2.0": {
                "dependencies": { "MyHotel.Entry": "0.1.0", "MyHotel.Infra": "0.1.0", "Scalar.AspNetCore": "2.0.0" }
              },
              "MyHotel.Contract/0.1.0": {},
              "MyHotel.Core/0.2.0": {
                "dependencies": { "Microsoft.Extensions.DependencyInjection.Abstractions": "10.0.0", "MyHotel.Contract": "0.1.0" }
              },
              "MyHotel.Entry/0.1.0": { "dependencies": { "MyHotel.Contract": "0.1.0" } },
              "MyHotel.Infra/0.1.0": { "dependencies": { "MyHotel.Core": "0.2.0" } },
              "Microsoft.Extensions.DependencyInjection.Abstractions/10.0.0": {},
              "Scalar.AspNetCore/2.0.0": {},
              "xunit.v3/4.0.1": {}
            }
          },
          "libraries": {
            "MyHotel.Spec/1.0.0": { "type": "project" },
            "MyHotel/0.2.0": { "type": "project" },
            "MyHotel.Contract/0.1.0": { "type": "project" },
            "MyHotel.Core/0.2.0": { "type": "project" },
            "MyHotel.Entry/0.1.0": { "type": "project" },
            "MyHotel.Infra/0.1.0": { "type": "project" },
            "Microsoft.Extensions.DependencyInjection.Abstractions/10.0.0": { "type": "package" },
            "Scalar.AspNetCore/2.0.0": { "type": "package" },
            "xunit.v3/4.0.1": { "type": "package" }
          }
        }
        """;
}
