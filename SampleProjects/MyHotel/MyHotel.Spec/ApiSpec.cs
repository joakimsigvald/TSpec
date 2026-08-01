using Microsoft.AspNetCore.Mvc.Testing;

namespace MyHotel.Spec;

/// <summary>
/// Base for black-box specifications: the subject under test is an <see cref="HttpClient"/>
/// wired to an in-memory instance of the API.
/// </summary>
/// <remarks>
/// A fresh application per test, not a shared one. The API keeps its state in memory, so a shared
/// instance would let one test's rooms be visible to the next. Both the application and its client
/// are owned by the pipeline and disposed with the test.
/// </remarks>
public abstract class ApiSpec<TResult> : Spec<HttpClient, TResult>
{
    protected ApiSpec()
    {
        var api = new WebApplicationFactory<Program>();
        Using(api, owned: true);
        Using(api.CreateClient, owned: true);
    }
}
