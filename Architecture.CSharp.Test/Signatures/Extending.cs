namespace TSpec.Architecture.CSharp.Test.Signatures;

public static class Extending
{
    extension(Vehicle vehicle)
    {
        public bool IsMoving => false;
    }
}
