using IshTop.Application.Jobs.Commands.SaveJob;
using IshTop.Application.Jobs.Queries.GetJobsByDateRange;
using IshTop.Application.Jobs.Queries.GetSavedJobs;
using IshTop.Application.Users.Commands.CreateUser;
using IshTop.Bot.Services;
using IshTop.Domain.Enums;
using IshTop.Domain.Interfaces.Repositories;
using IshTop.Shared.Constants;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace IshTop.Bot.Handlers;

public class UpdateHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly OnboardingService _onboarding;
    private readonly JobDisplayService _jobDisplay;
    private readonly ILogger<UpdateHandler> _logger;

    public UpdateHandler(ITelegramBotClient bot, IMediator mediator, IUnitOfWork unitOfWork,
        OnboardingService onboarding, JobDisplayService jobDisplay, ILogger<UpdateHandler> logger)
    {
        _bot = bot;
        _mediator = mediator;
        _unitOfWork = unitOfWork;
        _onboarding = onboarding;
        _jobDisplay = jobDisplay;
        _logger = logger;
    }

    public async Task HandleAsync(Update update, CancellationToken ct)
    {
        if (update.Message is { } message)
            await HandleMessageAsync(message, ct);
        else if (update.CallbackQuery is { } callback)
            await HandleCallbackAsync(callback, ct);
    }

    // =========================================================================
    // MESSAGE HANDLER
    // =========================================================================

    private async Task HandleMessageAsync(Message message, CancellationToken ct)
    {
        if (message.Text is null || message.From is null) return;

        var chatId = message.Chat.Id;
        var text = message.Text.Trim();
        var telegramUser = message.From;

        // Foydalanuvchini olish yoki yaratish
        var user = await _unitOfWork.Users.GetWithProfileByTelegramIdAsync(telegramUser.Id, ct);
        if (user is null)
        {
            await _mediator.Send(new CreateUserCommand(
                telegramUser.Id, telegramUser.Username,
                telegramUser.FirstName, telegramUser.LastName), ct);
            user = await _unitOfWork.Users.GetWithProfileByTelegramIdAsync(telegramUser.Id, ct);
        }
        if (user is null) return;

        var isUz = user.Language == LanguagePreference.Uzbek;

        // /start — har doim onboarding'ni qayta boshlaydi
        if (text == "/start")
        {
            await _bot.SendMessage(chatId,
                isUz ? BotMessages.Uz.Welcome : BotMessages.Ru.Welcome,
                cancellationToken: ct);
            user.State = UserState.New;
            user.OnboardingStep = OnboardingStep.Language;
            await _unitOfWork.SaveChangesAsync(ct);
            await _onboarding.HandleStepAsync(user, "", chatId, ct);
            return;
        }

        // Onboarding davom etayotgan bo'lsa — barcha matnlar onboarding'ga ketadi
        if (user.State is UserState.New or UserState.Onboarding)
        {
            await _onboarding.ProcessAnswerAsync(user, text, chatId, ct);
            return;
        }

        // =====================================================================
        // ASOSIY MENYU TUGMALARI (ReplyKeyboard)
        // =====================================================================
        if (text == BotMessages.Uz.BtnJobs || text == BotMessages.Ru.BtnJobs)
        {
            await SendJobsSubmenu(chatId, isUz, ct);
            return;
        }

        if (text == BotMessages.Uz.BtnProfile || text == BotMessages.Ru.BtnProfile)
        {
            await SendProfilePage(user, chatId, isUz, ct);
            return;
        }

        // Noma'lum matn — asosiy menyuni ko'rsatish
        await SendMainMenu(chatId, isUz, ct);
    }

    // =========================================================================
    // CALLBACK HANDLER
    // =========================================================================

    private async Task HandleCallbackAsync(CallbackQuery callback, CancellationToken ct)
    {
        if (callback.Data is null || callback.Message is null) return;

        var chatId = callback.Message.Chat.Id;
        var data = callback.Data;

        var user = await _unitOfWork.Users.GetWithProfileByTelegramIdAsync(callback.From.Id, ct);
        if (user is null) return;

        var isUz = user.Language == LanguagePreference.Uzbek;

        try
        {
            // ------------------------------------------------------------------
            // ONBOARDING callbacklari
            // ------------------------------------------------------------------
            if (data.StartsWith("lang:"))
            {
                var lang = data == "lang:uz" ? "O'zbek" : "Русский";
                await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
                await _onboarding.ProcessAnswerAsync(user, lang, chatId, ct);
                return;
            }

            if (data.StartsWith("exp:"))
            {
                await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
                await _onboarding.ProcessAnswerAsync(user, data[4..], chatId, ct);
                return;
            }

            if (data.StartsWith("work:"))
            {
                await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
                await _onboarding.ProcessAnswerAsync(user, data[5..], chatId, ct);
                return;
            }

            if (data.StartsWith("city:"))
            {
                var city = data[5..];
                if (city == "other")
                {
                    await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
                    await _bot.SendMessage(chatId,
                        isUz ? "Shaharni yozing:" : "Напишите город:", cancellationToken: ct);
                    return;
                }
                await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
                await _onboarding.ProcessAnswerAsync(user, city, chatId, ct);
                return;
            }

            if (data.StartsWith("eng:"))
            {
                await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
                await _onboarding.ProcessAnswerAsync(user, data[4..], chatId, ct);
                return;
            }

            if (data.StartsWith("confirm:"))
            {
                var answer = data == "confirm:yes" ? "✅ Ha" : "Yo'q";
                await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
                await _onboarding.ProcessAnswerAsync(user, answer, chatId, ct);
                return;
            }

            // ------------------------------------------------------------------
            // ISH SUBMENU
            // ------------------------------------------------------------------
            if (data == "jobs_menu")
            {
                await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
                await SendJobsSubmenu(chatId, isUz, ct);
                return;
            }

            // Sevimlilar
            if (data.StartsWith("jobs_saved:") && int.TryParse(data["jobs_saved:".Length..], out var savedPage))
            {
                await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
                var saved = await _mediator.Send(new GetSavedJobsQuery(user.TelegramId, savedPage), ct);
                await _jobDisplay.SendPaginatedJobsAsync(chatId, saved, user.Language, "jobs_saved", ct);
                return;
            }

            // Yangilar (oxirgi 3 kun) — pagination
            if (data.StartsWith("jobs_new:") && int.TryParse(data["jobs_new:".Length..], out var newPage))
            {
                await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
                var newJobs = await _mediator.Send(new GetJobsByDateRangeQuery("3days", newPage), ct);
                await _jobDisplay.SendPaginatedJobsAsync(chatId, newJobs, user.Language, "jobs_new", ct);
                return;
            }

            // Barchasi — vaqt oralig'i filtri pagination
            // Format: jobs_3days:PAGE, jobs_week:PAGE, jobs_2weeks:PAGE, jobs_month:PAGE
            if (TryParseJobsDateCallback(data, out var dateRange, out var datePage))
            {
                await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
                var filtered = await _mediator.Send(new GetJobsByDateRangeQuery(dateRange, datePage), ct);
                await _jobDisplay.SendPaginatedJobsAsync(chatId, filtered, user.Language, $"jobs_{dateRange}", ct);
                return;
            }

            // ------------------------------------------------------------------
            // ISH SUBMENU tugmalari (InlineKeyboard'dan)
            // ------------------------------------------------------------------
            if (data == "jobs_show_saved")
            {
                await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
                var saved = await _mediator.Send(new GetSavedJobsQuery(user.TelegramId, 1), ct);
                await _jobDisplay.SendPaginatedJobsAsync(chatId, saved, user.Language, "jobs_saved", ct);
                return;
            }

            if (data == "jobs_show_new")
            {
                await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
                var newJobs = await _mediator.Send(new GetJobsByDateRangeQuery("3days", 1), ct);
                await _jobDisplay.SendPaginatedJobsAsync(chatId, newJobs, user.Language, "jobs_new", ct);
                return;
            }

            if (data == "jobs_show_all")
            {
                await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
                await SendDateRangeMenu(chatId, isUz, ct);
                return;
            }

            // Vaqt oralig'ini tanlash
            if (data.StartsWith("date_range:"))
            {
                var range = data["date_range:".Length..];
                await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
                var jobs = await _mediator.Send(new GetJobsByDateRangeQuery(range, 1), ct);
                await _jobDisplay.SendPaginatedJobsAsync(chatId, jobs, user.Language, $"jobs_{range}", ct);
                return;
            }

            // ------------------------------------------------------------------
            // E'LON SAQLASH
            // ------------------------------------------------------------------
            if (data.StartsWith("save:") && Guid.TryParse(data[5..], out var saveJobId))
            {
                var result = await _mediator.Send(new SaveJobCommand(user.TelegramId, saveJobId), ct);
                await _bot.AnswerCallbackQuery(callback.Id,
                    result.IsSuccess
                        ? (isUz ? BotMessages.Uz.JobSaved : BotMessages.Ru.JobSaved)
                        : (isUz ? BotMessages.Uz.JobAlreadySaved : BotMessages.Ru.JobAlreadySaved),
                    showAlert: false,
                    cancellationToken: ct);
                return;
            }

            // ------------------------------------------------------------------
            // SOZLAMALAR
            // ------------------------------------------------------------------
            if (data == "settings_menu")
            {
                await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
                await SendSettingsMenu(user, chatId, isUz, ct);
                return;
            }

            if (data == "settings_notif")
            {
                user.NotificationsEnabled = !user.NotificationsEnabled;
                await _unitOfWork.SaveChangesAsync(ct);
                await _bot.AnswerCallbackQuery(callback.Id,
                    user.NotificationsEnabled
                        ? (isUz ? BotMessages.Uz.NotificationsOn : BotMessages.Ru.NotificationsOn)
                        : (isUz ? BotMessages.Uz.NotificationsOff : BotMessages.Ru.NotificationsOff),
                    cancellationToken: ct);
                // Sozlamalar menyusini yangilash
                await SendSettingsMenu(user, chatId, isUz, ct);
                return;
            }

            if (data == "settings_lang")
            {
                await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
                var keyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("🇺🇿 O'zbek tili", "settings_lang:uz"),
                        InlineKeyboardButton.WithCallbackData("🇷🇺 Русский язык", "settings_lang:ru")
                    }
                });
                await _bot.SendMessage(chatId,
                    isUz ? BotMessages.Uz.SelectLanguage : BotMessages.Ru.SelectLanguage,
                    replyMarkup: keyboard, cancellationToken: ct);
                return;
            }

            if (data.StartsWith("settings_lang:"))
            {
                var langCode = data["settings_lang:".Length..];
                user.Language = langCode == "uz" ? LanguagePreference.Uzbek : LanguagePreference.Russian;
                await _unitOfWork.SaveChangesAsync(ct);
                isUz = user.Language == LanguagePreference.Uzbek;
                await _bot.AnswerCallbackQuery(callback.Id,
                    isUz ? "Til o'zgartirildi 🇺🇿" : "Язык изменён 🇷🇺",
                    cancellationToken: ct);
                await SendMainMenu(chatId, isUz, ct);
                return;
            }

            if (data == "settings_edit_profile")
            {
                await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
                user.State = UserState.New;
                user.OnboardingStep = OnboardingStep.TechStack; // Tilni saqlagan holda qaytadan
                await _unitOfWork.SaveChangesAsync(ct);
                await _onboarding.HandleStepAsync(user, "", chatId, ct);
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Callback '{Data}' handling error", data);
            try { await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct); } catch { }
        }
    }

    // =========================================================================
    // UI HELPER METHODS
    // =========================================================================

    /// <summary>Asosiy menyu — 2 ta ReplyKeyboard tugmasi: Ish va Profil.</summary>
    public async Task SendMainMenu(long chatId, bool isUz, CancellationToken ct)
    {
        var keyboard = new ReplyKeyboardMarkup(new[]
        {
            new[]
            {
                new KeyboardButton(isUz ? BotMessages.Uz.BtnJobs : BotMessages.Ru.BtnJobs),
                new KeyboardButton(isUz ? BotMessages.Uz.BtnProfile : BotMessages.Ru.BtnProfile)
            }
        })
        {
            ResizeKeyboard = true,
            IsPersistent = true
        };

        await _bot.SendMessage(chatId,
            isUz ? BotMessages.Uz.MainMenu : BotMessages.Ru.MainMenu,
            replyMarkup: keyboard,
            cancellationToken: ct);
    }

    /// <summary>Ish bo'limi — Sevimlilar / Yangilar / Barchasi InlineKeyboard.</summary>
    private async Task SendJobsSubmenu(long chatId, bool isUz, CancellationToken ct)
    {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    isUz ? BotMessages.Uz.BtnSaved : BotMessages.Ru.BtnSaved, "jobs_show_saved"),
                InlineKeyboardButton.WithCallbackData(
                    isUz ? BotMessages.Uz.BtnNew : BotMessages.Ru.BtnNew, "jobs_show_new")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    isUz ? BotMessages.Uz.BtnAll : BotMessages.Ru.BtnAll, "jobs_show_all")
            }
        });

        await _bot.SendMessage(chatId,
            isUz ? BotMessages.Uz.JobsMenu : BotMessages.Ru.JobsMenu,
            replyMarkup: keyboard,
            cancellationToken: ct);
    }

    /// <summary>Barchasi — vaqt oralig'i tanlash menyusi.</summary>
    private async Task SendDateRangeMenu(long chatId, bool isUz, CancellationToken ct)
    {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    isUz ? BotMessages.Uz.BtnLast3Days : BotMessages.Ru.BtnLast3Days, "date_range:3days"),
                InlineKeyboardButton.WithCallbackData(
                    isUz ? BotMessages.Uz.BtnLastWeek : BotMessages.Ru.BtnLastWeek, "date_range:week")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    isUz ? BotMessages.Uz.BtnLast2Weeks : BotMessages.Ru.BtnLast2Weeks, "date_range:2weeks"),
                InlineKeyboardButton.WithCallbackData(
                    isUz ? BotMessages.Uz.BtnLastMonth : BotMessages.Ru.BtnLastMonth, "date_range:month")
            }
        });

        await _bot.SendMessage(chatId,
            isUz ? BotMessages.Uz.SelectDateRange : BotMessages.Ru.SelectDateRange,
            replyMarkup: keyboard,
            cancellationToken: ct);
    }

    /// <summary>Profil sahifasi — ma'lumotlar + Sozlamalar tugmasi.</summary>
    private async Task SendProfilePage(Domain.Entities.User user, long chatId, bool isUz, CancellationToken ct)
    {
        var settingsKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    isUz ? "⚙️ Sozlamalar" : "⚙️ Настройки", "settings_menu")
            }
        });

        if (user.Profile?.IsComplete == true)
        {
            var p = user.Profile;
            var text = isUz
                ? $"👤 <b>Profilingiz</b>\n\n" +
                  $"💻 <b>Texnologiyalar:</b> {string.Join(", ", p.TechStacks)}\n" +
                  $"📊 <b>Tajriba:</b> {p.ExperienceLevel}\n" +
                  $"💰 <b>Maosh:</b> {p.SalaryMin}–{p.SalaryMax} {p.Currency}\n" +
                  $"🏠 <b>Ish turi:</b> {p.WorkType}\n" +
                  $"📍 <b>Shahar:</b> {p.City}\n" +
                  $"🌍 <b>Ingliz tili:</b> {p.EnglishLevel}\n" +
                  $"🔔 <b>Bildirishnomalar:</b> {(user.NotificationsEnabled ? "Yoqilgan" : "O'chirilgan")}"
                : $"👤 <b>Ваш профиль</b>\n\n" +
                  $"💻 <b>Технологии:</b> {string.Join(", ", p.TechStacks)}\n" +
                  $"📊 <b>Опыт:</b> {p.ExperienceLevel}\n" +
                  $"💰 <b>Зарплата:</b> {p.SalaryMin}–{p.SalaryMax} {p.Currency}\n" +
                  $"🏠 <b>Тип работы:</b> {p.WorkType}\n" +
                  $"📍 <b>Город:</b> {p.City}\n" +
                  $"🌍 <b>Английский:</b> {p.EnglishLevel}\n" +
                  $"🔔 <b>Уведомления:</b> {(user.NotificationsEnabled ? "Включены" : "Отключены")}";

            await _bot.SendMessage(chatId, text,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                replyMarkup: settingsKeyboard,
                cancellationToken: ct);
        }
        else
        {
            await _bot.SendMessage(chatId,
                isUz ? BotMessages.Uz.ProfileNotComplete : BotMessages.Ru.ProfileNotComplete,
                replyMarkup: settingsKeyboard,
                cancellationToken: ct);
        }
    }

    /// <summary>Sozlamalar menyusi — til, bildirishnoma, profil tahrirlash.</summary>
    private async Task SendSettingsMenu(Domain.Entities.User user, long chatId, bool isUz, CancellationToken ct)
    {
        var notifBtn = user.NotificationsEnabled
            ? (isUz ? "🔔 Bildirishnomalar: Yoqilgan" : "🔔 Уведомления: Включены")
            : (isUz ? "🔕 Bildirishnomalar: O'chirilgan" : "🔕 Уведомления: Отключены");

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData(notifBtn, "settings_notif") },
            new[] { InlineKeyboardButton.WithCallbackData(isUz ? "🌐 Tilni o'zgartirish" : "🌐 Изменить язык", "settings_lang") },
            new[] { InlineKeyboardButton.WithCallbackData(isUz ? "✏️ Profilni tahrirlash" : "✏️ Редактировать профиль", "settings_edit_profile") }
        });

        await _bot.SendMessage(chatId,
            isUz ? BotMessages.Uz.SettingsMenu : BotMessages.Ru.SettingsMenu,
            replyMarkup: keyboard,
            cancellationToken: ct);
    }

    // =========================================================================
    // PRIVATE HELPERS
    // =========================================================================

    /// <summary>
    /// "jobs_3days:2", "jobs_week:1", "jobs_2weeks:3", "jobs_month:1" formatidagi
    /// callbacklarni parse qiladi.
    /// </summary>
    private static bool TryParseJobsDateCallback(string data, out string dateRange, out int page)
    {
        dateRange = "";
        page = 1;

        string[] prefixes = ["jobs_3days:", "jobs_week:", "jobs_2weeks:", "jobs_month:"];
        string[] ranges   = ["3days",       "week",      "2weeks",       "month"];

        for (var i = 0; i < prefixes.Length; i++)
        {
            if (data.StartsWith(prefixes[i]) && int.TryParse(data[prefixes[i].Length..], out var p))
            {
                dateRange = ranges[i];
                page = p;
                return true;
            }
        }
        return false;
    }
}
