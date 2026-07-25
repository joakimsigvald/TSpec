using TSpec.Assert;

namespace TSpec.Test.Given;

public class WhenGivenBools : Spec<bool[]> 
{
    [Fact] public void GivenThreeBool_ThenGetThreeBools() => Three<bool>().Has().Count(3);
}