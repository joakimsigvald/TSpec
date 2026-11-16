namespace TSpec.Architecture.CSharp.Test.Signatures;

public class Vehicle;

public class Car : Vehicle;

public class Convertible : Car;

public class Malfunction : InvalidOperationException;

public class Wrapper<T>;

public class Wrapped : Wrapper<int>;
