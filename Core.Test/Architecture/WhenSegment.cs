using TSpec.Architecture;
using TSpec.Assert;

namespace TSpec.Test.Architecture;

public class WhenSegment : Spec
{
    [Fact] public void ThenTakeTheSegmentAtTheIndex() => "Formats.Fhir".Segment(0).Is("Formats");

    [Fact] public void GivenAnIndexFromTheEnd_ThenCountFromTheEnd() => "Formats.Fhir".Segment(^1).Is("Fhir");

    [Fact] public void GivenNoSegmentAtTheIndex_ThenTakeEmpty() => "Entry".Segment(1).Is(string.Empty);

    [Fact] public void GivenARange_ThenJoinItsSegments() => "Data.Auth.Store".Segment(..2).Is("Data.Auth");

    [Fact] public void GivenARangePastTheEnd_ThenTakeEmpty() => "Entry".Segment(..2).Is(string.Empty);
}
