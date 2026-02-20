# IshTop: Tizim Arxitekturasi

Ushbu hujjat "IshTop" loyihasi uchun arxitektura va texnologik qarorlarni tavsiflaydi. Loyiha O'zbekiston bozori uchun mo'ljallangan, Telegramga asoslangan ish topish platformasi bo'lib, kelajakda kengaytiriladi.

## 1. Umumiy Arxitektura (High-Level Overview)

Tizim modulli monolit (Modular Monolith) sifatida boshlanadi, lekin mikroservislarga ajratish oson bo'lishi uchun Clean Architecture tamoyillariga va Domain-Driven Design (DDD) yondashuviga asoslanadi.

![System Overview](https://placeholder-image-url-for-diagram)
*(Diagramma o'rnida: User -> Telegram Bot -> API Gateway -> Core Services -> Database)*

### Asosiy Komponentlar:
1.  **Telegram Bot (User Interface)**: Foydalanuvchilar va ish beruvchilar uchun asosiy interfeys.
2.  **API Gateway / Backend (Core System)**: .NET 8 Web API. Barcha biznes logika shu yerda.
3.  **Job Parser Service**: Telegram kanallardan ish e'lonlarini yig'uvchi alohida xizmat.
4.  **Admin Panel**: Next.js (React) asosidagi web-interfeys moderatorlar uchun.
5.  **AI Engine**: OpenAI (yoki open-source LLM) integratsiyasi - resume tahlili va ish moslash uchun.

---

## 2. Texnologik Stack

-   **Backend**: .NET 8 (C#) - Yuqori unumdorlik va barqarorlik uchun.
-   **Database**: PostgreSQL - Asosiy ma'lumotlar ombori (Relational + JSONB).
-   **Cache**: Redis - Tezkor kirish va vaqtinchalik ma'lumotlar (User Session, Hot Jobs) uchun.
-   **Message Broker**: RabbitMQ - Servislararo asinxron aloqa (Event-Driven) uchun.
-   **Search Engine**: Elasticsearch / PostgreSQL Full-Text Search - Ish qidirish tizimi uchun.
-   **AI Integration**: OpenAI API / Local LLM - Resume parsing va matching uchun.
-   **Admin Frontend**: Next.js (React) - Zamonaviy va tezkor UI.
-   **Containerization**: Docker & Docker Compose - Oson deploy va development muhiti uchun.

---

## 3. Ma'lumotlar Oqimi (Data Flow)

### 3.1. Foydalanuvchi Ro'yxatdan O'tishi (User Flow)
1.  Foydalanuvchi botga `/start` bosadi.
2.  Bot API orqali foydalanuvchini tekshiradi.
3.  AI yordamida suhbat (chat) orqali resume yig'iladi (Ism, Stack, Tajriba, Maosh).
4.  Ma'lumotlar strukturalashtirilib, PostgreSQL ga saqlanadi.
5.  Mos keluvchi ishlar (Matching Engine) qidiriladi va Redis da keshlanadi.

### 3.2. Ish E'lonlarini Yig'ish (Parser Flow)
1.  Parser xizmati belgilangan Telegram kanallarni kuzatadi.
2.  Yangi xabar kelganda, spam ekanligi tekshiriladi (Regex + AI).
3.  Haqiqiy ish e'loni bo'lsa, xom matn (Raw Text) RabbitMQ ga yuboriladi.
4.  Core Service xabarni oladi va AI ga yuboradi.
5.  AI e'londan maydonlarni (Kompaniya, Maosh, Talablar) ajratib oladi.
6.  Tozalanagan e'lon bazaga yoziladi va obunachilarga (Notification Service) yuboriladi.

---

## 4. Loyiha Tuzilishi (Solution Structure)

Biz **Clean Architecture** qoidalariga amal qilamiz:

```
src/
├── IshTop.Domain/          # Enterprise Logic (Entities, Value Objects, Enums) - No dependencies
├── IshTop.Application/     # Business Logic (UseCases, DTOs, Interfaces) - Depends on Domain
├── IshTop.Infrastructure/  # External Services (DB, Redis, AI, Telegram API) - Implements Interfaces
├── IshTop.Api/             # Entry Point (Controllers, Middleware) - Depends on Application & Infrastructure
├── IshTop.Bot/             # Telegram Bot Interface - Uses Application layer
└── IshTop.Parser/          # Standalone Worker Service for parsing
```

---

## 5. Scalability & Performance (O'lchovchanlik)

-   **Horizontal Scaling**: API va Bot stateles (holatsiz) bo'lib, yuklama oshganda nusxalarini ko'paytirishimiza mumkin (Kubernetes/Docker Swarm).
-   **Caching Strategy**:
    -   Tez-tez so'raladigan ma'lumotlar (Regions, Categories) -> In-Memory Cache.
    -   Foydalanuvchi sessiyasi va vaqtinchalik holatlar -> Redis.
-   **Database Optimization**:
    -   Indexlash (ayniqsa JSONB ustunlar va qidiruv so'zlari uchun).
    -   Read Replica (o'qish uchun alohida nusxa) kelajakda qo'shilishi mumkin.
-   **Asynchronous Processing**:
    -   Og'ir vazifalar (AI tahlil, Broadcast xabarlar) orqa fonda RabbitMQ orqali bajariladi. Foydalanuvchi kutib qolmaydi.

---

## 6. Xavfsizlik (Security)

-   **Authentication**: Telegram Login Widget yoki Telegram Data Hash validatsiyasi.
-   **Authorization**: Admin va Moderator rollari (RBAC).
-   **Data Protection**: Foydalanuvchi shaxsiy ma'lumotlari (telefon raqam) shifrlangan yoki himoyalangan holda saqlanadi.
-   **Rate Limiting**: API va Botga haddan tashqari ko'p so'rov yuborishni cheklash.

---

## 7. Xulosa

Ushbu arxitektura "IshTop" loyihasini tez ishga tushirish (MVP) va keyinchalik katta yuklamalarga dosh berish (Scaling) imkonini beradi. .NET 8 va Telegram ekotizimi O'zbekiston sharoitida eng optimal va arzon yechim hisoblanadi.
