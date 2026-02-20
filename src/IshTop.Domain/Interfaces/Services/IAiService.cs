using IshTop.Domain.Entities;

namespace IshTop.Domain.Interfaces.Services;

public interface IAiService
{
    Task<UserProfile> ExtractProfileFromConversationAsync(IEnumerable<string> messages, CancellationToken ct = default);
    Task<Job> ParseJobFromMessageAsync(string rawText, CancellationToken ct = default);
    Task<bool> DetectSpamAsync(string text, CancellationToken ct = default);
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default);
}
