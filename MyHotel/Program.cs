using System.Reflection;
using Scalar.AspNetCore;

var version = Assembly.GetExecutingAssembly()
    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
    .InformationalVersion.Split('+')[0];

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/version", () => new VersionInfo(version));

app.Run();

public record VersionInfo(string Version);

public partial class Program;
