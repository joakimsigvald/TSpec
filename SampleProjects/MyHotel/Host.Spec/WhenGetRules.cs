using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec;

public class WhenGetRules : ApiSpec<HttpResponseMessage>
{
    public WhenGetRules() => When(_ => _.Api.GetAsync("/rules"));

    [Fact] public void ThenRespondOk() => Result.StatusCode.Is(OK);

    [Fact]
    public async Task ThenReturnTheBedCountLimits() => (await Result.Read<Rules>()).Is(new Rules(1, 9));
}
