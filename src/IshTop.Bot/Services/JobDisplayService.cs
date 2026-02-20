using IshTop.Application.Common.Models;
using IshTop.Application.Jobs.DTOs;
using IshTop.Domain.Enums;
using IshTop.Shared.Constants;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace IshTop.Bot.Services;

public class JobDisplayService
{
    private readonly ITelegramBotClient _bot;

    public JobDisplayService(ITelegramBotClient bot) => _bot = bot;

    /// <summary>
    /// Bitta e'lonni forward-like ko'rinishda inline tugmalar bilan yuboradi.
    /// Faqat ❤️ Saqlash tugmasi bor — user saqlash uchun bosadi.
    /// </summary>
    public async Task SendJobCardAsync(long chatId, JobDto job, LanguagePreference lang, CancellationToken ct)
    {
        var isUz = lang == LanguagePreference.Uzbek;
        var text = BuildJobText(job, isUz);

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    isUz ? "❤️ Saqlash" : "❤️ Сохранить",
                    $"save:{job.Id}")
            }
        });

        await _bot.SendMessage(
            chatId,
            text,
            parseMode: ParseMode.Html,
            replyMarkup: keyboard,
            cancellationToken: ct);
    }

    /// <summary>
    /// PaginatedList'dagi e'lonlarni ketma-ket yuboradi va oxirida pagination navigatsiyasini ko'rsatadi.
    /// paginationPrefix — callback prefix, masalan "jobs_new", "jobs_saved", "jobs_3days"
    /// </summary>
    public async Task SendPaginatedJobsAsync(
        long chatId,
        PaginatedList<JobDto> result,
        LanguagePreference lang,
        string paginationPrefix,
        CancellationToken ct)
    {
        var isUz = lang == LanguagePreference.Uzbek;

        if (result.Items.Count == 0)
        {
            var emptyMsg = paginationPrefix.StartsWith("jobs_saved")
                ? (isUz ? BotMessages.Uz.SavedJobsEmpty : BotMessages.Ru.SavedJobsEmpty)
                : (isUz ? BotMessages.Uz.NoJobsInRange : BotMessages.Ru.NoJobsInRange);

            await _bot.SendMessage(chatId, emptyMsg, cancellationToken: ct);
            return;
        }

        // E'lonlarni ketma-ket yuborish
        foreach (var job in result.Items)
        {
            await SendJobCardAsync(chatId, job, lang, ct);
            await Task.Delay(200, ct);
        }

        // Agar bir nechta sahifa bo'lsa — navigatsiya tugmalari
        if (result.TotalPages > 1)
        {
            var navButtons = new List<InlineKeyboardButton>();

            if (result.HasPreviousPage)
                navButtons.Add(InlineKeyboardButton.WithCallbackData(
                    isUz ? BotMessages.Uz.BtnPrev : BotMessages.Ru.BtnPrev,
                    $"{paginationPrefix}:{result.Page - 1}"));

            if (result.HasNextPage)
                navButtons.Add(InlineKeyboardButton.WithCallbackData(
                    isUz ? BotMessages.Uz.BtnNext : BotMessages.Ru.BtnNext,
                    $"{paginationPrefix}:{result.Page + 1}"));

            var pageText = string.Format(
                isUz ? BotMessages.Uz.PageInfo : BotMessages.Ru.PageInfo,
                result.Page, result.TotalPages);

            await _bot.SendMessage(
                chatId,
                pageText,
                replyMarkup: new InlineKeyboardMarkup(navButtons),
                cancellationToken: ct);
        }
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    private static string BuildJobText(JobDto job, bool isUz)
    {
        var parts = new List<string>();

        if (job.IsFeatured) parts.Add("⭐ <b>Featured</b>");

        parts.Add($"<b>{Escape(job.Title)}</b>");

        if (job.Company is not null)
            parts.Add($"🏢 {Escape(job.Company)}");

        parts.Add("");

        // Maosh
        if (job.SalaryMin.HasValue)
            parts.Add($"💰 {job.SalaryMin}–{job.SalaryMax} {job.Currency}");
        else
            parts.Add(isUz ? "💰 Ko'rsatilmagan" : "💰 Не указано");

        if (job.ExperienceLevel.HasValue)
            parts.Add($"📊 {job.ExperienceLevel}");

        if (job.WorkType.HasValue)
            parts.Add($"🏠 {job.WorkType}");

        if (job.Location is not null)
            parts.Add($"📍 {Escape(job.Location)}");

        if (job.TechStacks.Count > 0)
            parts.Add($"💻 {Escape(string.Join(", ", job.TechStacks))}");

        parts.Add("");

        // Tavsif — 400 belgiga qisqartiriladi
        var desc = job.Description.Length > 400
            ? job.Description[..400] + "..."
            : job.Description;
        parts.Add(Escape(desc));

        // Murojaat ma'lumoti
        if (job.ContactInfo is not null)
        {
            parts.Add("");
            parts.Add(isUz
                ? $"📬 <b>Murojaat:</b> {Escape(job.ContactInfo)}"
                : $"📬 <b>Контакт:</b> {Escape(job.ContactInfo)}");
        }

        parts.Add("");

        // E'lon sanasi
        var dateStr = job.CreatedAt.ToString("dd.MM.yyyy HH:mm");
        parts.Add(isUz ? $"🕐 {dateStr}" : $"🕐 {dateStr}");

        // Kanal manbasi — forward o'xshash ko'rinish
        if (job.SourceMessageId.HasValue)
        {
            var channelLabel = isUz ? "Manba" : "Источник";
            if (job.ChannelUsername is not null)
            {
                // Public kanal — @username/messageId
                var username = job.ChannelUsername.TrimStart('@');
                var link = $"https://t.me/{username}/{job.SourceMessageId}";
                parts.Add($"📢 <b>{channelLabel}:</b> <a href=\"{link}\">{Escape(job.ChannelTitle ?? $"@{username}")}</a>");
            }
            else if (job.ChannelTitle is not null && !job.ChannelTitle.StartsWith("Channel_"))
            {
                // Private kanal — faqat nom
                parts.Add($"📢 <b>{channelLabel}:</b> {Escape(job.ChannelTitle)}");
            }
            // Channel_ID bo'lsa ko'rsatmaymiz
        }

        return string.Join("\n", parts);
    }

    private static string Escape(string text) =>
        text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}
