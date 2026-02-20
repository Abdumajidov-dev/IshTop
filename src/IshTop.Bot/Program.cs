using IshTop.Application;
using IshTop.Bot.Handlers;
using IshTop.Bot.Services;
using IshTop.Infrastructure;
using Telegram.Bot;

var builder = Host.CreateApplicationBuilder(args);

// Add layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Telegram Bot
builder.Services.AddSingleton<ITelegramBotClient>(sp =>
    new TelegramBotClient(builder.Configuration["Telegram:BotToken"]
        ?? throw new InvalidOperationException("Telegram:BotToken is not configured")));

// Bot services
builder.Services.AddScoped<UpdateHandler>();
builder.Services.AddScoped<OnboardingService>();
builder.Services.AddScoped<JobDisplayService>();

// Background service
builder.Services.AddHostedService<BotBackgroundService>();

var host = builder.Build();
host.Run();
