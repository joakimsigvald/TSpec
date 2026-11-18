using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using MyHotel.Contract;
using MyHotel.Contract.Bookings;
using MyHotel.Contract.Bookings.Exceptions;
using MyHotel.Contract.Rooms.Exceptions;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace MyHotel.Entry;

/// <summary>
/// Turns the exceptions Contract declares into the refusals they stand for: a status and an error
/// code. Anything else is left alone: returning false hands it back to the pipeline, which logs it
/// and answers 500. Catching it here would only replace a real diagnostic with a guess.
/// </summary>
internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (RefusalFor(exception) is not { } refusal)
            return false;

        context.Response.StatusCode = refusal.Status;
        await context.Response.WriteAsJsonAsync(new ErrorBody(refusal.Code), cancellationToken);
        return true;
    }

    private static Refusal? RefusalFor(Exception exception) =>
        exception switch
        {
            RoomNotFound => new(Status404NotFound, nameof(RoomNotFound)),
            RoomAlreadyExists => new(Status409Conflict, nameof(RoomAlreadyExists)),
            RoomNumberMismatch => new(Status400BadRequest, nameof(RoomNumberMismatch)),
            RoomHasOpenBookings => new(Status409Conflict, nameof(RoomHasOpenBookings)),
            RoomAlreadyBooked => new(Status409Conflict, nameof(RoomAlreadyBooked)),
            InvalidBookingPeriod => new(Status400BadRequest, nameof(InvalidBookingPeriod)),
            BookingNotFound => new(Status404NotFound, nameof(BookingNotFound)),
            BookingNotOpen => new(Status409Conflict, nameof(BookingNotOpen)),
            _ => null,
        };

    private readonly record struct Refusal(int Status, string Code);
}
