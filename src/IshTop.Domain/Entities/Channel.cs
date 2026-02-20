using IshTop.Domain.Common;

namespace IshTop.Domain.Entities;

public class Channel : BaseEntity
{
    public long TelegramId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Username { get; set; }
    public bool IsActive { get; set; } = true;
    public int JobCount { get; set; }
    public DateTime? LastParsedAt { get; set; }

    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
