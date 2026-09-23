// See https://aka.ms/new-console-template for more information

using Microsoft.AspNetCore.Builder;
using ModelContextProtocol.Server;


var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5050");

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<IntentClassifierTools>();

builder.Services.AddSingleton<PlanStore>();
var app = builder.Build();

app.MapMcp("/mcp");

app.Run();