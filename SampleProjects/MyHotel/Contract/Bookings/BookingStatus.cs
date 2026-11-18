using System.Text.Json.Serialization;

namespace MyHotel.Contract.Bookings;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BookingStatus
{
    Open,
    Past,
    Cancelled,
}
