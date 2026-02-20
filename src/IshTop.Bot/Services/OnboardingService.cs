using IshTop.Application.Users.Commands.UpdateProfile;
using IshTop.Domain.Entities;
using IshTop.Domain.Enums;
using IshTop.Domain.Interfaces.Repositories;
using IshTop.Shared.Constants;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace IshTop.Bot.Services;

public class OnboardingService
{
    private readonly ITelegramBotClient _bot;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public OnboardingService(ITelegramBotClient bot, IUnitOfWork unitOfWork, IMediator mediator)
    {
        _bot = bot;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task HandleStepAsync(User user, string text, long chatId, CancellationToken ct)
    {
        switch (user.OnboardingStep)
        {
            case OnboardingStep.Language:
                await AskLanguage(chatId, ct);
                break;

            case OnboardingStep.TechStack:
                await _bot.SendMessage(chatId,
                    user.Language == LanguagePreference.Uzbek ? BotMessages.Uz.AskTechStack : BotMessages.Ru.AskTechStack,
                    cancellationToken: ct);
                break;

            case OnboardingStep.Experience:
                await AskExperience(chatId, user.Language, ct);
                break;

            case OnboardingStep.Salary:
                await _bot.SendMessage(chatId,
                    user.Language == LanguagePreference.Uzbek ? BotMessages.Uz.AskSalary : BotMessages.Ru.AskSalary,
                    cancellationToken: ct);
                break;

            case OnboardingStep.WorkType:
                await AskWorkType(chatId, user.Language, ct);
                break;

            case OnboardingStep.City:
                await AskCity(chatId, user.Language, ct);
                break;

            case OnboardingStep.EnglishLevel:
                await AskEnglishLevel(chatId, user.Language, ct);
                break;

            case OnboardingStep.Confirmation:
                await ShowProfileConfirmation(user, chatId, ct);
                break;
        }
    }

    public async Task ProcessAnswerAsync(User user, string text, long chatId, CancellationToken ct)
    {
        var profile = user.Profile ?? new UserProfile { UserId = user.Id };
        user.Profile ??= profile;

        switch (user.OnboardingStep)
        {
            case OnboardingStep.Language:
                user.Language = text.Contains("O'zbek") || text.Contains("Uzbek")
                    ? LanguagePreference.Uzbek
                    : LanguagePreference.Russian;
                user.OnboardingStep = OnboardingStep.TechStack;
                break;

            case OnboardingStep.TechStack:
                profile.TechStacks = text.Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
                user.OnboardingStep = OnboardingStep.Experience;
                break;

            case OnboardingStep.Experience:
                if (Enum.TryParse<ExperienceLevel>(text, true, out var exp))
                    profile.ExperienceLevel = exp;
                user.OnboardingStep = OnboardingStep.Salary;
                break;

            case OnboardingStep.Salary:
                ParseSalary(text, profile);
                user.OnboardingStep = OnboardingStep.WorkType;
                break;

            case OnboardingStep.WorkType:
                if (Enum.TryParse<WorkType>(text, true, out var wt))
                    profile.WorkType = wt;
                user.OnboardingStep = OnboardingStep.City;
                break;

            case OnboardingStep.City:
                profile.City = text.Trim();
                user.OnboardingStep = OnboardingStep.EnglishLevel;
                break;

            case OnboardingStep.EnglishLevel:
                if (Enum.TryParse<EnglishLevel>(text, true, out var eng))
                    profile.EnglishLevel = eng;
                user.OnboardingStep = OnboardingStep.Confirmation;
                break;

            case OnboardingStep.Confirmation:
                if (text.Contains("Ha") || text.Contains("\u0414\u0430") || text.Contains("Yes") || text.Contains("\u2705"))
                {
                    await _mediator.Send(new UpdateProfileCommand(
                        user.TelegramId,
                        profile.TechStacks,
                        profile.ExperienceLevel,
                        profile.SalaryMin,
                        profile.SalaryMax,
                        profile.Currency,
                        profile.WorkType,
                        profile.City,
                        profile.EnglishLevel), ct);

                    var isUzLang = user.Language == LanguagePreference.Uzbek;

                    // Profil saqlangandan so'ng asosiy menyuni ko'rsatish
                    var mainKeyboard = new ReplyKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            new KeyboardButton(isUzLang ? BotMessages.Uz.BtnJobs : BotMessages.Ru.BtnJobs),
                            new KeyboardButton(isUzLang ? BotMessages.Uz.BtnProfile : BotMessages.Ru.BtnProfile)
                        }
                    })
                    {
                        ResizeKeyboard = true,
                        IsPersistent = true
                    };

                    await _bot.SendMessage(chatId,
                        isUzLang ? BotMessages.Uz.ProfileSaved : BotMessages.Ru.ProfileSaved,
                        replyMarkup: mainKeyboard,
                        cancellationToken: ct);
                    return;
                }
                else
                {
                    user.OnboardingStep = OnboardingStep.TechStack;
                }
                break;
        }

        user.State = UserState.Onboarding;
        await _unitOfWork.SaveChangesAsync(ct);
        await HandleStepAsync(user, text, chatId, ct);
    }

    private async Task AskLanguage(long chatId, CancellationToken ct)
    {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("\ud83c\uddfa\ud83c\uddff O'zbek tili", "lang:uz") },
            new[] { InlineKeyboardButton.WithCallbackData("\ud83c\uddf7\ud83c\uddfa \u0420\u0443\u0441\u0441\u043a\u0438\u0439 \u044f\u0437\u044b\u043a", "lang:ru") }
        });

        await _bot.SendMessage(chatId, BotMessages.Uz.ChooseLanguage, replyMarkup: keyboard, cancellationToken: ct);
    }

    private async Task AskExperience(long chatId, LanguagePreference lang, CancellationToken ct)
    {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("\ud83c\udf31 Intern", "exp:Intern"), InlineKeyboardButton.WithCallbackData("\ud83d\udc76 Junior", "exp:Junior") },
            new[] { InlineKeyboardButton.WithCallbackData("\ud83d\udcbc Middle", "exp:Middle"), InlineKeyboardButton.WithCallbackData("\ud83c\udfc6 Senior", "exp:Senior") },
            new[] { InlineKeyboardButton.WithCallbackData("\ud83d\udc51 Lead", "exp:Lead") }
        });

        await _bot.SendMessage(chatId,
            lang == LanguagePreference.Uzbek ? BotMessages.Uz.AskExperience : BotMessages.Ru.AskExperience,
            replyMarkup: keyboard, cancellationToken: ct);
    }

    private async Task AskWorkType(long chatId, LanguagePreference lang, CancellationToken ct)
    {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("\ud83c\udfe0 Remote", "work:Remote") },
            new[] { InlineKeyboardButton.WithCallbackData("\ud83c\udfe2 Office", "work:Office") },
            new[] { InlineKeyboardButton.WithCallbackData("\ud83d\udd04 Hybrid", "work:Hybrid") }
        });

        await _bot.SendMessage(chatId,
            lang == LanguagePreference.Uzbek ? BotMessages.Uz.AskWorkType : BotMessages.Ru.AskWorkType,
            replyMarkup: keyboard, cancellationToken: ct);
    }

    private async Task AskCity(long chatId, LanguagePreference lang, CancellationToken ct)
    {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Toshkent", "city:Toshkent"), InlineKeyboardButton.WithCallbackData("Samarqand", "city:Samarqand") },
            new[] { InlineKeyboardButton.WithCallbackData("Buxoro", "city:Buxoro"), InlineKeyboardButton.WithCallbackData("Farg'ona", "city:Farg'ona") },
            new[] { InlineKeyboardButton.WithCallbackData("Andijon", "city:Andijon"), InlineKeyboardButton.WithCallbackData("Namangan", "city:Namangan") },
            new[] { InlineKeyboardButton.WithCallbackData("Boshqa / \u0414\u0440\u0443\u0433\u043e\u0439", "city:other") }
        });

        await _bot.SendMessage(chatId,
            lang == LanguagePreference.Uzbek ? BotMessages.Uz.AskCity : BotMessages.Ru.AskCity,
            replyMarkup: keyboard, cancellationToken: ct);
    }

    private async Task AskEnglishLevel(long chatId, LanguagePreference lang, CancellationToken ct)
    {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("\u274c None", "eng:None"), InlineKeyboardButton.WithCallbackData("\ud83d\udcd6 Beginner", "eng:Beginner") },
            new[] { InlineKeyboardButton.WithCallbackData("\ud83d\udcdd Intermediate", "eng:Intermediate"), InlineKeyboardButton.WithCallbackData("\ud83d\udcda Advanced", "eng:Advanced") },
            new[] { InlineKeyboardButton.WithCallbackData("\ud83c\udf1f Fluent", "eng:Fluent") }
        });

        await _bot.SendMessage(chatId,
            lang == LanguagePreference.Uzbek ? BotMessages.Uz.AskEnglish : BotMessages.Ru.AskEnglish,
            replyMarkup: keyboard, cancellationToken: ct);
    }

    private async Task ShowProfileConfirmation(User user, long chatId, CancellationToken ct)
    {
        var p = user.Profile!;
        var isUz = user.Language == LanguagePreference.Uzbek;

        var text = isUz
            ? $"\ud83d\udccb Sizning profilingiz:\n\n\ud83d\udcbb Texnologiyalar: {string.Join(", ", p.TechStacks)}\n\ud83d\udcca Tajriba: {p.ExperienceLevel}\n\ud83d\udcb0 Maosh: {p.SalaryMin}-{p.SalaryMax} {p.Currency}\n\ud83c\udfe0 Ish turi: {p.WorkType}\n\ud83d\udccd Shahar: {p.City}\n\ud83c\udf0d Ingliz tili: {p.EnglishLevel}\n\nTaskiqlaysizmi?"
            : $"\ud83d\udccb \u0412\u0430\u0448 \u043f\u0440\u043e\u0444\u0438\u043b\u044c:\n\n\ud83d\udcbb \u0422\u0435\u0445\u043d\u043e\u043b\u043e\u0433\u0438\u0438: {string.Join(", ", p.TechStacks)}\n\ud83d\udcca \u041e\u043f\u044b\u0442: {p.ExperienceLevel}\n\ud83d\udcb0 \u0417\u0430\u0440\u043f\u043b\u0430\u0442\u0430: {p.SalaryMin}-{p.SalaryMax} {p.Currency}\n\ud83c\udfe0 \u0422\u0438\u043f \u0440\u0430\u0431\u043e\u0442\u044b: {p.WorkType}\n\ud83d\udccd \u0413\u043e\u0440\u043e\u0434: {p.City}\n\ud83c\udf0d \u0410\u043d\u0433\u043b\u0438\u0439\u0441\u043a\u0438\u0439: {p.EnglishLevel}\n\n\u041f\u043e\u0434\u0442\u0432\u0435\u0440\u0436\u0434\u0430\u0435\u0442\u0435?";

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(isUz ? "\u2705 Ha" : "\u2705 \u0414\u0430", "confirm:yes"),
                InlineKeyboardButton.WithCallbackData(isUz ? "\ud83d\udd04 Qayta to'ldirish" : "\ud83d\udd04 \u0417\u0430\u043d\u043e\u0432\u043e", "confirm:no")
            }
        });

        await _bot.SendMessage(chatId, text, replyMarkup: keyboard, cancellationToken: ct);
    }

    private static void ParseSalary(string text, UserProfile profile)
    {
        text = text.Trim().ToUpper();

        if (text.Contains("USD") || text.Contains("$"))
            profile.Currency = Currency.USD;
        else if (text.Contains("UZS") || text.Contains("\u0421\u0423\u041c") || text.Contains("SO'M"))
            profile.Currency = Currency.UZS;

        var numbers = System.Text.RegularExpressions.Regex.Matches(text, @"\d+")
            .Select(m => decimal.Parse(m.Value)).ToList();

        if (numbers.Count >= 2)
        {
            profile.SalaryMin = numbers[0];
            profile.SalaryMax = numbers[1];
        }
        else if (numbers.Count == 1)
        {
            profile.SalaryMin = numbers[0];
            profile.SalaryMax = numbers[0];
        }
    }
}
