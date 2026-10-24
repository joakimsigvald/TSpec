namespace TSpec.Architecture.CSharp.Test.Signatures;

public enum Status { Open }

public record Booking(Status Status);

public abstract class Signing
{
    public abstract Status Held { get; }

    public abstract Status Plain();

    public abstract List<Status> Listed();

    public abstract (int, Status) Paired();

    public abstract Status[] Arrayed();

    public abstract Task<Status> Awaited();

    public abstract Status? Maybe();

    public abstract Booking Wrapped();

    public abstract void Nothing();

    public abstract IEnumerable<int> Sequence();

    public abstract List<int> Listing();

    public abstract Task<(int, int)> AwaitedPair();

    public abstract void Taking(List<Status> statuses, int count);

    public abstract (Status, Status) Twice();

    public abstract void TakingTwice(Status first, Status second);
}
