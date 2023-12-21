using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using PaymentHub.PaymentInterfaces.Services;
using PaymentHub.Services;
using PIXItau.Services;
using System.Net;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureAppConfiguration((webHostBuilderContext, configurationbuilder) =>
{
    var env = webHostBuilderContext.HostingEnvironment;
    configurationbuilder.SetBasePath(env.ContentRootPath);
    configurationbuilder.AddJsonFile("appsettings.json", optional: true, true);
    configurationbuilder.AddJsonFile($"secrets/appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true); //injected on kubernets secret AKS
    configurationbuilder.AddEnvironmentVariables();
});

builder.WebHost.ConfigureKestrel((context, serverOptions) =>
{
    serverOptions.Listen(IPAddress.Any, 80, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});
// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddSingleton<IPIXInterface, PIXItauService>();

builder.Services.AddGrpc();
var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<PaymentHubService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();