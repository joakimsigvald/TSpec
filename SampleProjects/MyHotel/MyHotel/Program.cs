using System.Reflection;
using MyHotel.Core;
using MyHotel.Entry;
using Scalar.AspNetCore;

var version = Assembly.GetExecutingAssembly()
    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
    .InformationalVersion.Split('+')[0];

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddCore();
builder.Services.AddEntry();

var app = builder.Build();

app.UseExceptionHandler();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapVersionEndpoint(version);
app.MapRoomEndpoints();

app.Run();

public partial class Program;
