using System.Net;

namespace MyHotel.Spec;

public class WhenGetVersion : ApiSpec<HttpResponseMessage>
{
    public WhenGetVersion() => When(api => api.GetAsync("/version"));

    [Fact]
    public void ThenRespondOk()
    {
        Result.StatusCode.Is(HttpStatusCode.OK);
        Specification.Is(
            """
            Using owned CreateClient
            When api.GetAsync("/version")
            Then Result.StatusCode is HttpStatusCode.OK
            """);
    }

    [Fact]
    public async Task ThenReturnTheApplicationVersion()
        => (await Result.Read<VersionInfo>()).Version.Is(DeclaredVersion);

    /// <summary>
    /// Read from the assembly version, which the SDK sets from &lt;Version&gt; in MyHotel.csproj.
    /// The endpoint reads the informational version, so agreeing means both track the project file.
    /// </summary>
    private static string DeclaredVersion
        => typeof(Program).Assembly.GetName().Version!.ToString(3);
}
