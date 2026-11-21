using System.Text.Json.Serialization;

namespace MyHotel.Contract.Bookings;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BookingAction
{
    Cancel,
    CheckIn,
    CheckOut,
    Pay,
}
