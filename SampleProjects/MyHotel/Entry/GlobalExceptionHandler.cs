using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using MyHotel.Contract;

namespace MyHotel.Entry;

/// <summary>
/// Turns the exceptions Contract declares into the responses they stand for. Anything else is left
/// alone: returning false hands it back to the pipeline, which logs it and answers 500. Catching it
/// here would only replace a real diagnostic with a guess.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var status = exception switch
        {
            RoomNotFound => StatusCodes.Status404NotFound,
            RoomAlreadyExists => StatusCodes.Status409Conflict,
            _ => 0
        };
        if (status is 0)
            return false;

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(
            new { error = exception.Message }, cancellationToken);
        return true;
    }
}
