using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/version", () => new VersionInfo("0.1.0"));

app.Run();

public record VersionInfo(string Version);

public partial class Program;
