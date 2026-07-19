using Compliance360.Application.Notifications;
using Compliance360.Infrastructure;
using Compliance360.Worker;

var builder = Host.CreateApplicationBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Compliance360");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("ConnectionStrings:Compliance360 must be configured before starting the Alert Center worker.");
}

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "O";
    options.UseUtcTimestamp = true;
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<AlertCenterWorker>();

var host = builder.Build();
await host.RunAsync();

