namespace TSpec.Test.Internal.Document;

/// <summary>
/// A dependency manifest shaped like the real thing: one direct project reference (MyHotel),
/// one direct package reference, and one project reference that is only transitive.
/// </summary>
internal static class DepsJson
{
    internal const string _myHotelSpec =
        """
        {
          "runtimeTarget": { "name": ".NETCoreApp,Version=v10.0" },
          "targets": {
            ".NETCoreApp,Version=v10.0": {
              "MyHotel.Spec/1.0.0": {
                "dependencies": {
                  "MyHotel": "0.1.0",
                  "TSpec": "1.5.0",
                  "xunit.v3": "3.2.2"
                }
              },
              "MyHotel/0.1.0": { "dependencies": { "MyHotel.Persistence": "0.1.0" } },
              "MyHotel.Persistence/0.1.0": {},
              "TSpec/1.5.0": {},
              "xunit.v3/3.2.2": {}
            }
          },
          "libraries": {
            "MyHotel.Spec/1.0.0": { "type": "project" },
            "MyHotel/0.1.0": { "type": "project" },
            "MyHotel.Persistence/0.1.0": { "type": "project" },
            "TSpec/1.5.0": { "type": "package" },
            "xunit.v3/3.2.2": { "type": "package" }
          }
        }
        """;
}
