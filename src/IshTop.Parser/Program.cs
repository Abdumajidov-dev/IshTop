using IshTop.Application;
using IshTop.Infrastructure;
using IshTop.Parser.Services;

var builder = Host.CreateApplicationBuilder(args);

// Add layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Parser services
builder.Services.AddHostedService<ChannelMonitorService>();

var host = builder.Build();
host.Run();
