namespace TSpec.Test.Subjects.RecordStructDefaults;

public record struct Key<TA, TB>
{
    public TA A { get; set; }
    public TB B { get; set; }
}