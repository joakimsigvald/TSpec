namespace TSpec.Architecture.CSharp.Test.Signatures;

public interface IRoom
{
    int Number { get; }

    event Action? Opened;

    void Open();
}

public interface IClosable
{
    void Close();
}

public interface IStoried
{
    int Floors { get; }
}

public abstract class RoomBase
{
    public abstract void Clean();

    public virtual string Describe() => "";
}

public class Room : RoomBase, IRoom, IClosable
{
    private readonly int _guests = 2;

    public Room() => Guests = _guests;

    public event Action? Opened;

    public int Number => 1;

    public int Guests { get; }

    public void Open() => Opened?.Invoke();

    void IClosable.Close() { }

    public override void Clean() { }

    public override string ToString() => "";

    public void Book() { }
}

public record Suite(int Floors) : IStoried;
