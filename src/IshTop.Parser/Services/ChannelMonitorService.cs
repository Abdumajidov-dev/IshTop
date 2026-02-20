using IshTop.Application.Jobs.Commands.CreateJob;
using IshTop.Domain.Entities;
using IshTop.Domain.Interfaces.Repositories;
using MediatR;
using TL;
using WTelegram;

namespace IshTop.Parser.Services;

public class ChannelMonitorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChannelMonitorService> _logger;
    private Client? _client;

    public ChannelMonitorService(IServiceProvider serviceProvider, IConfiguration configuration,
        ILogger<ChannelMonitorService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _client = new Client(ConfigProvider);
            _client.OnUpdates += OnUpdatesAsync;

            var user = await _client.LoginUserIfNeeded();
            _logger.LogInformation("Parser logged in as: {Name} ({Id})", user.first_name, user.id);

            // Startup: barcha a'zo bo'lgan kanallardan oxirgi xabarlarni o'qi
            await BackfillAllChannelsAsync(stoppingToken);

            _logger.LogInformation("Backfill completed. Listening for new messages...");

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Parser service failed");
        }
        finally
        {
            _client?.Dispose();
        }
    }

    /// <summary>
    /// Login bo'lgach barcha kanallardagi so'nggi xabarlarni o'qib bazaga qo'shadi.
    /// </summary>
    private async Task BackfillAllChannelsAsync(CancellationToken ct)
    {
        if (_client is null) return;

        try
        {
            _logger.LogInformation("Starting backfill from all joined channels...");

            // A'zo bo'lgan barcha dialog/kanallarni olish
            var dialogs = await _client.Messages_GetAllDialogs();

            var channels = dialogs.chats.Values
                .OfType<TL.Channel>()
                .Where(c => !c.IsGroup) // faqat kanallar (guruhlar emas)
                .ToList();

            _logger.LogInformation("Found {Count} channels to backfill", channels.Count);

            foreach (var channel in channels)
            {
                if (ct.IsCancellationRequested) break;
                await BackfillChannelAsync(channel, ct);
                await Task.Delay(500, ct); // Telegram rate limit uchun
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Backfill failed");
        }
    }

    private async Task BackfillChannelAsync(TL.Channel channel, CancellationToken ct)
    {
        if (_client is null) return;

        try
        {
            // Kanal bazada bor bo'lsa username/title ni yangilab qo'yamiz
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var dbChannel = await unitOfWork.Channels.GetByTelegramIdAsync(channel.id);
                var username = string.IsNullOrEmpty(channel.username) ? null : $"@{channel.username}";
                if (dbChannel is null)
                {
                    dbChannel = new Domain.Entities.Channel
                    {
                        TelegramId = channel.id,
                        Title = channel.title,
                        Username = username,
                        IsActive = true
                    };
                    await unitOfWork.Channels.AddAsync(dbChannel);
                    await unitOfWork.SaveChangesAsync();
                }
                else if (dbChannel.Username != username || dbChannel.Title != channel.title)
                {
                    dbChannel.Username = username;
                    dbChannel.Title = channel.title;
                    await unitOfWork.SaveChangesAsync();
                }
            }

            var inputChannel = new InputChannel(channel.id, channel.access_hash);
            var history = await _client.Messages_GetHistory(inputChannel, limit: 200);

            var messages = history.Messages
                .OfType<Message>()
                .Where(m => !string.IsNullOrWhiteSpace(m.message))
                .ToList();

            _logger.LogInformation("Backfilling channel {Title}: {Count} messages", channel.title, messages.Count);

            foreach (var msg in messages)
            {
                if (ct.IsCancellationRequested) break;
                await ProcessChannelMessageAsync(msg, channel.id);
                await Task.Delay(100, ct); // OpenAI rate limit uchun
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to backfill channel {Title}", channel.title);
        }
    }

    private async Task OnUpdatesAsync(UpdatesBase updates)
    {
        foreach (var update in updates.UpdateList)
        {
            if (update is UpdateNewChannelMessage { message: Message message })
            {
                long channelId = 0;
                if (message.peer_id is PeerChannel pc)
                    channelId = pc.channel_id;

                await ProcessChannelMessageAsync(message, channelId);
            }
        }
    }

    private async Task ProcessChannelMessageAsync(Message message, long channelId)
    {
        if (string.IsNullOrWhiteSpace(message.message)) return;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // Kanal bazada borligini tekshir, yo'q bo'lsa qo'sh
            if (channelId != 0)
            {
                var channel = await unitOfWork.Channels.GetByTelegramIdAsync(channelId);
                if (channel is null)
                {
                    channel = new Domain.Entities.Channel
                    {
                        TelegramId = channelId,
                        Title = $"Channel_{channelId}",
                        IsActive = true
                    };
                    await unitOfWork.Channels.AddAsync(channel);
                    await unitOfWork.SaveChangesAsync();
                }
            }

            var result = await mediator.Send(new CreateJobCommand(
                message.message,
                channelId,
                message.id));

            if (result.IsSuccess)
                _logger.LogInformation("Job saved from channel {ChannelId}, msg {MessageId}", channelId, message.id);
            else
                _logger.LogDebug("Skipped msg {MessageId}: {Reason}", message.id, result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message {MessageId}", message.id);
        }
    }

    private static string? ReadFromConsole(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine();
    }

    private string? ConfigProvider(string what)
    {
        return what switch
        {
            "api_id" => _configuration["Telegram:ApiId"],
            "api_hash" => _configuration["Telegram:ApiHash"],
            "phone_number" => _configuration["Telegram:PhoneNumber"],
            "verification_code" => _configuration["Telegram:VerificationCode"] ?? ReadFromConsole("Enter Telegram verification code: "),
            "password" => _configuration["Telegram:TwoFAPassword"] ?? ReadFromConsole("Enter 2FA password: "),
            _ => null
        };
    }
}
