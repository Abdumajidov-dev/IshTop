using System.Text.Json;
using IshTop.Domain.Entities;
using IshTop.Domain.Enums;
using IshTop.Domain.Interfaces.Services;
using IshTop.Infrastructure.AI.Prompts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using OpenAI.Embeddings;

namespace IshTop.Infrastructure.AI;

public class OpenAiService : IAiService
{
    private readonly ChatClient _chatClient;
    private readonly EmbeddingClient _embeddingClient;
    private readonly ICacheService _cache;
    private readonly ILogger<OpenAiService> _logger;

    public OpenAiService(IConfiguration configuration, ICacheService cache, ILogger<OpenAiService> logger)
    {
        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI:ApiKey is not configured");

        _chatClient = new ChatClient("gpt-4o-mini", apiKey);
        _embeddingClient = new EmbeddingClient("text-embedding-3-small", apiKey);
        _cache = cache;
        _logger = logger;
    }

    public async Task<Job> ParseJobFromMessageAsync(string rawText, CancellationToken ct = default)
    {
        var prompt = PromptTemplates.JobParsing + rawText;

        var completion = await _chatClient.CompleteChatAsync(
            [new UserChatMessage(prompt)],
            new ChatCompletionOptions { Temperature = 0.1f },
            ct);

        var json = ExtractJson(completion.Value.Content[0].Text);
        var parsed = JsonSerializer.Deserialize<JobParseResult>(json, JsonOptions);

        var job = new Job
        {
            Title = parsed?.Title ?? "Nomsiz vakansiya",
            Description = parsed?.Description ?? rawText,
            Company = parsed?.Company,
            TechStacks = parsed?.TechStacks ?? [],
            ExperienceLevel = ParseEnum<ExperienceLevel>(parsed?.ExperienceLevel),
            SalaryMin = parsed?.SalaryMin,
            SalaryMax = parsed?.SalaryMax,
            Currency = ParseEnum<Currency>(parsed?.Currency),
            WorkType = ParseEnum<WorkType>(parsed?.WorkType),
            Location = parsed?.Location,
            ContactInfo = parsed?.ContactInfo,
            RawText = rawText
        };

        return job;
    }

    public async Task<bool> DetectSpamAsync(string text, CancellationToken ct = default)
    {
        var prompt = PromptTemplates.SpamDetection + text;

        var completion = await _chatClient.CompleteChatAsync(
            [new UserChatMessage(prompt)],
            new ChatCompletionOptions { Temperature = 0f },
            ct);

        var result = completion.Value.Content[0].Text.Trim().ToLower();
        return result == "true";
    }

    public async Task<UserProfile> ExtractProfileFromConversationAsync(IEnumerable<string> messages, CancellationToken ct = default)
    {
        var conversation = string.Join("\n", messages);
        var prompt = PromptTemplates.ProfileExtraction + conversation;

        var completion = await _chatClient.CompleteChatAsync(
            [new UserChatMessage(prompt)],
            new ChatCompletionOptions { Temperature = 0.1f },
            ct);

        var json = ExtractJson(completion.Value.Content[0].Text);
        var parsed = JsonSerializer.Deserialize<ProfileParseResult>(json, JsonOptions);

        return new UserProfile
        {
            TechStacks = parsed?.TechStacks ?? [],
            ExperienceLevel = ParseEnum<ExperienceLevel>(parsed?.ExperienceLevel) ?? ExperienceLevel.Junior,
            SalaryMin = parsed?.SalaryMin,
            SalaryMax = parsed?.SalaryMax,
            Currency = ParseEnum<Currency>(parsed?.Currency) ?? Currency.USD,
            WorkType = ParseEnum<WorkType>(parsed?.WorkType) ?? WorkType.Remote,
            City = parsed?.City,
            EnglishLevel = ParseEnum<EnglishLevel>(parsed?.EnglishLevel) ?? EnglishLevel.None,
            IsComplete = true
        };
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        var cacheKey = $"embedding:{text.GetHashCode()}";
        var cached = await _cache.GetAsync<float[]>(cacheKey, ct);
        if (cached is not null) return cached;

        var embedding = await _embeddingClient.GenerateEmbeddingAsync(text, cancellationToken: ct);
        var result = embedding.Value.ToFloats().ToArray();

        await _cache.SetAsync(cacheKey, result, TimeSpan.FromDays(7), ct);
        return result;
    }

    private static string ExtractJson(string text)
    {
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start >= 0 && end > start)
            return text[start..(end + 1)];
        return text;
    }

    private static T? ParseEnum<T>(string? value) where T : struct, Enum
    {
        if (string.IsNullOrEmpty(value)) return null;
        return Enum.TryParse<T>(value, true, out var result) ? result : null;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private record JobParseResult(
        string? Title, string? Description, string? Company,
        List<string>? TechStacks, string? ExperienceLevel,
        decimal? SalaryMin, decimal? SalaryMax, string? Currency,
        string? WorkType, string? Location, string? ContactInfo,
        bool IsJobPost);

    private record ProfileParseResult(
        List<string>? TechStacks, string? ExperienceLevel,
        decimal? SalaryMin, decimal? SalaryMax, string? Currency,
        string? WorkType, string? City, string? EnglishLevel);
}
