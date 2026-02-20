using IshTop.Domain.Common;

namespace IshTop.Domain.Entities;

public class HiddenJob : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid JobId { get; set; }
    public Job Job { get; set; } = null!;
}
