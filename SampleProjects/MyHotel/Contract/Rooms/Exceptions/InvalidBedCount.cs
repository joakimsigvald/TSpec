namespace MyHotel.Contract.Rooms.Exceptions;

public class InvalidBedCount(int bedCount)
    : Exception(
        $"A room must have from {BedCountLimits.Min} to {BedCountLimits.Max} beds: {bedCount} is not.")
{
    public int BedCount { get; } = bedCount;
}
