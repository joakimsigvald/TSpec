namespace TSpec.Internal.Pipelines;

internal enum Phase { Declare, Arrange, Act, Assert }

/// What the pipeline builds reads the phase; only the pipeline advances it.
internal interface IPipelinePhase
{
    Phase Current { get; }
}

internal sealed class PipelinePhase : IPipelinePhase
{
    public Phase Current { get; private set; }

    internal void AdvanceTo(Phase next)
    {
        if (next != Current + 1)
            throw new InvalidOperationException(
                $"Cannot advance the pipeline from {Current} to {next}. A pipeline runs once, so do not run it "
                + "again after it failed; if the test does not, this is a bug in TSpec. "
                + "Please report it at https://github.com/joakimsigvald/TSpec/issues");

        Current = next;
    }
}
