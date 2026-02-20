using IshTop.Domain.Common;
using IshTop.Domain.Enums;

namespace IshTop.Domain.Entities;

public class User : BaseEntity
{
    public long TelegramId { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public LanguagePreference Language { get; set; } = LanguagePreference.Uzbek;
    public UserState State { get; set; } = UserState.New;
    public OnboardingStep OnboardingStep { get; set; } = OnboardingStep.Language;
    public bool NotificationsEnabled { get; set; } = true;

    public UserProfile? Profile { get; set; }
    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    public ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
    public ICollection<HiddenJob> HiddenJobs { get; set; } = new List<HiddenJob>();
}
