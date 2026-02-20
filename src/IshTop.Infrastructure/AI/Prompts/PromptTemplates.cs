namespace IshTop.Infrastructure.AI.Prompts;

public static class PromptTemplates
{
    public const string JobParsing = """
        Sen Uzbekistondagi IT ish e'lonlarini tahlil qiluvchi AI assistantsan.
        Quyidagi xabarni tahlil qilib, JSON formatida javob ber.
        Agar ma'lumot topilmasa null qo'y.

        JSON format:
        {
            "title": "Lavozim nomi",
            "description": "Ish tavsifi",
            "company": "Kompaniya nomi yoki null",
            "techStacks": ["texnologiya1", "texnologiya2"],
            "experienceLevel": "Intern|Junior|Middle|Senior|Lead yoki null",
            "salaryMin": raqam yoki null,
            "salaryMax": raqam yoki null,
            "currency": "UZS|USD yoki null",
            "workType": "Remote|Office|Hybrid yoki null",
            "location": "Shahar yoki null",
            "contactInfo": "Aloqa ma'lumoti yoki null",
            "isJobPost": true/false
        }

        Xabar:
        """;

    public const string SpamDetection = """
        Sen Telegram kanallaridagi xabarlarni spam/reklama ekanligini aniqlash uchun AI assistantsan.
        Quyidagi xabar ish e'loni emas, spam/reklama yoki boshqa kontentmi?

        Faqat "true" (spam) yoki "false" (spam emas) deb javob ber.

        Xabar:
        """;

    public const string ProfileExtraction = """
        Sen foydalanuvchi bilan suhbat asosida kasbiy profilni tuzuvchi AI assistantsan.
        Quyidagi suhbat asosida JSON formatida profil yarat.

        JSON format:
        {
            "techStacks": ["texnologiya1", "texnologiya2"],
            "experienceLevel": "Intern|Junior|Middle|Senior|Lead",
            "salaryMin": raqam yoki null,
            "salaryMax": raqam yoki null,
            "currency": "UZS|USD",
            "workType": "Remote|Office|Hybrid",
            "city": "Shahar nomi yoki null",
            "englishLevel": "None|Beginner|Intermediate|Advanced|Fluent"
        }

        Suhbat:
        """;
}
