using System.Net;

namespace MyHotel.Spec;

public abstract class WhenGetVersion : ApiSpec<HttpResponseMessage>
{
    public class GivenNothing : WhenGetVersion
    {
        public GivenNothing() => When(api => api.GetAsync("/version"));

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
        public async Task ThenReturnTheApiVersion()
            => (await Result.Read<VersionInfo>()).Version.Is("0.1.0");
    }
}
