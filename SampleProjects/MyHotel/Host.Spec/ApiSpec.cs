namespace MyHotel.Spec;

/// <summary>
/// Base for black-box specifications: the subject under test is the running application, reached
/// over HTTP. A fresh <see cref="Hotel"/> per test, each with its own room store, constructed and
/// disposed by the pipeline.
/// </summary>
public abstract class ApiSpec<TResult> : Spec<Hotel, TResult>;
