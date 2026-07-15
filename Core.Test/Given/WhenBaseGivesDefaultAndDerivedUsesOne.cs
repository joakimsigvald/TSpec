using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Given;

public abstract class WhenBaseGivesDefaultAndDerivedUsesOne : Spec<MyService, MyModel[]>
{
    protected const string BaseDefaultName = "BaseDefault";

    protected WhenBaseGivesDefaultAndDerivedUsesOne()
        => When(_ => _.GetModels())
        .Given<MyModel>(_ => _ with { Name = BaseDefaultName });

    public class AndDerivedConstructorEvaluatesOneWithTransform : WhenBaseGivesDefaultAndDerivedUsesOne
    {
        public AndDerivedConstructorEvaluatesOneWithTransform()
            => Using(One<MyModel>(_ => _ with { Id = 42 }));

        [Fact]
        public void ThenBaseDefaultNameIsAppliedToTheReferencedModel()
        {
            Then();
            The<MyModel>().Name.Is(BaseDefaultName);
            The<MyModel>().Id.Is(42);
        }
    }
}