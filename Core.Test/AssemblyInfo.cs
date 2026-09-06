using System.Globalization;
using System.Runtime.CompilerServices;
using TSpec;

//[assembly: AssemblyFixture(typeof(SpecificationDocument))]

namespace TSpec.Test;

/// <summary>
/// The suite runs in a culture that renders dates and numbers differently from the way TSpec does,
/// so anything that reads the ambient culture fails here rather than on a user's machine.
/// </summary>
/// <remarks>
/// This machine is sv-SE, which writes dates the same way TSpec's own convention does. That
/// coincidence hid a leak for as long as the suite ran in it: a record's generated ToString
/// rendered its members in the ambient culture, and only a run in another culture said so.
/// en-US disagrees about the date order, the time of day and the decimal separator, so it does.
/// </remarks>
internal static class SuiteCulture
{
    [ModuleInitializer]
    internal static void SetForeign()
    {
        var foreign = new CultureInfo("en-US");
        CultureInfo.DefaultThreadCurrentCulture = foreign;
        CultureInfo.DefaultThreadCurrentUICulture = foreign;
    }
}
