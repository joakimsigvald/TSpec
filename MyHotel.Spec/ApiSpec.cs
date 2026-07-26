using Microsoft.AspNetCore.Mvc.Testing;

namespace MyHotel.Spec;

/// <summary>
/// Base for black-box specifications: the subject under test is an <see cref="HttpClient"/>
/// wired to an in-memory instance of the API.
/// </summary>
public abstract class ApiSpec<TResult> : Spec<HttpClient, TResult>
{
    private static readonly WebApplicationFactory<Program> _api = new();

    protected ApiSpec() => Using(CreateClient, owned: true);

    private static HttpClient CreateClient() => _api.CreateClient();
}
