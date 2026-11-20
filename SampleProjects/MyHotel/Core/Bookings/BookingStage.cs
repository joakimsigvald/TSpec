using System.Text.Json.Serialization;

namespace MyHotel.Core.Bookings;

/// <summary>
/// What has happened to a booking, as the store keeps it, so that it can never be both checked in and
/// cancelled. Stored by name, so the file reads as what happened.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BookingStage
{
    Booked,
    CheckedIn,
    CheckedOut,
    Cancelled,
}
