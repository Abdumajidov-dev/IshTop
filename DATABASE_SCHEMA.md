# IshTop: Ma'lumotlar Bazasi Sxemasi (Database Schema)

Ushbu hujjat "IshTop" loyihasi uchun PostgreSQL ma'lumotlar bazasi sxemasini tavsiflaydi.

## 1. Asosiy Jadvallar (Core Tables)

### 1.1. `users` (Foydalanuvchilar)
Telegram foydalanuvchilarining asosiy ma'lumotlari.

| Field Name | Type | Constraints | Description |
| :--- | :--- | :--- | :--- |
| `id` | `uuid` | PK, NOT NULL | Unique User ID |
| `telegram_id` | `bigint` | UNIQUE, NOT NULL | Telegram User ID |
| `username` | `text` | NULLABLE | Telegram Username (nullable) |
| `first_name` | `text` | NULLABLE | Telegram First Name |
| `last_name` | `text` | NULLABLE | Telegram Last Name |
| `phone_number` | `text` | NULLABLE | Verified phone number |
| `role` | `enum` | DEFAULT 'candidate' | 'candidate', 'recruiter', 'admin' |
| `language_code` | `varchar(10)` | DEFAULT 'uz' | Interface language preference |
| `created_at` | `timestamptz` | DEFAULT NOW() | Registration date |
| `updated_at` | `timestamptz` | DEFAULT NOW() | Last profile update |
| `is_blocked` | `boolean` | DEFAULT FALSE | Account status |

### 1.2. `candidates` (Nomzodlar / Resumelar)
Foydalanuvchilarning ish qidirish profillari. `users` jadvali bilan 1:1 bog'liq.

| Field Name | Type | Constraints | Description |
| :--- | :--- | :--- | :--- |
| `user_id` | `uuid` | PK, FK(users.id) | Foreign Key to Users table |
| `full_name` | `text` | NOT NULL | User's full name for CV |
| `title` | `text` | NOT NULL | Desired Job Title (e.g. .NET Developer) |
| `experience_level` | `enum` | NOT NULL | 'junior', 'middle', 'senior', 'lead' |
| `skills` | `text[]` | NULLABLE | Array of skills (e.g. ['C#', 'SQL', 'React']) |
| `salary_min` | `decimal` | NULLABLE | Minimum salary expectation (USD/UZS converted) |
| `currency` | `varchar(3)` | DEFAULT 'USD' | Preferred salary currency |
| `location` | `text` | NULLABLE | Preferred city/region |
| `work_type` | `enum` | NULLABLE | 'remote', 'office', 'hybrid' |
| `bio` | `text` | NULLABLE | Brief introduction or summary |
| `contacts` | `jsonb` | NULLABLE | Additional contacts (LinkedIn, Github, Email) |
| `is_active` | `boolean` | DEFAULT TRUE | Looking for job status |

### 1.3. `companies` (Kompaniyalar)
Ish beruvchi kompaniyalar haqida ma'lumot.

| Field Name | Type | Constraints | Description |
| :--- | :--- | :--- | :--- |
| `id` | `uuid` | PK, NOT NULL | Unique Company ID |
| `name` | `text` | NOT NULL | Company Name |
| `logo_url` | `text` | NULLABLE | Company Logo URL |
| `website` | `text` | NULLABLE | Official Website |
| `industry` | `text` | NULLABLE | Industry sector (IT, Finance, etc.) |
| `description` | `text` | NULLABLE | About company |
| `created_at` | `timestamptz` | DEFAULT NOW() | Created date |

### 1.4. `jobs` (Ish E'lonlari)
E'lon qilingan vakansiyalar (Parsed yoki User Posted).

| Field Name | Type | Constraints | Description |
| :--- | :--- | :--- | :--- |
| `id` | `uuid` | PK, NOT NULL | Unique Job ID |
| `company_id` | `uuid` | FK(companies.id), NULLABLE | Linked Company (if known) |
| `recruiter_id` | `uuid` | FK(users.id), NULLABLE | Posted by user (recruiter) |
| `title` | `text` | NOT NULL | Job Title |
| `description` | `text` | NOT NULL | Full Job Description |
| `requirements` | `text[]` | NULLABLE | List of requirements |
| `salary_min` | `decimal` | NULLABLE | Min Salary |
| `salary_max` | `decimal` | NULLABLE | Max Salary |
| `currency` | `varchar(3)` | DEFAULT 'USD' | Salary Currency |
| `location` | `text` | NULLABLE | Job Location |
| `work_type` | `enum` | NULLABLE | 'remote', 'office', 'hybrid' |
| `experience_level` | `enum` | NULLABLE | Expected level |
| `source` | `enum` | DEFAULT 'manual' | 'manual', 'parsed' |
| `source_link` | `text` | NULLABLE | Original link (if parsed) |
| `status` | `enum` | DEFAULT 'active' | 'active', 'closed', 'draft', 'archived' |
| `created_at` | `timestamptz` | DEFAULT NOW() | Posted date |
| `expires_at` | `timestamptz` | NULLABLE | Expiration date |

### 1.5. `applications` (Arizalar / Mosliklar)
Nomzodlarning ishga topshirgan arizalari yoki mosliklari.

| Field Name | Type | Constraints | Description |
| :--- | :--- | :--- | :--- |
| `id` | `uuid` | PK, NOT NULL | Unique Application ID |
| `job_id` | `uuid` | FK(jobs.id), NOT NULL | Related Job |
| `candidate_id` | `uuid` | FK(users.id), NOT NULL | Candidate User |
| `status` | `enum` | DEFAULT 'applied' | 'applied', 'viewed', 'interview', 'rejected', 'offer' |
| `match_score` | `decimal` | NULLABLE | AI Calculated Match Score (0-100) |
| `cover_letter` | `text` | NULLABLE | Optional cover letter |
| `applied_at` | `timestamptz` | DEFAULT NOW() | Application timestamp |

### 1.6. `subscriptions` (Obunalar)
Monetizatsiya va Premium xizmatlar uchun.

| Field Name | Type | Constraints | Description |
| :--- | :--- | :--- | :--- |
| `id` | `uuid` | PK, NOT NULL | Unique Subscription ID |
| `user_id` | `uuid` | FK(users.id), NOT NULL | Subscriber |
| `plan_type` | `enum` | NOT NULL | 'premium_user', 'premium_recruiter' |
| `start_date` | `timestamptz` | NOT NULL | Start Date |
| `end_date` | `timestamptz` | NOT NULL | End Date |
| `is_active` | `boolean` | DEFAULT TRUE | Status |
| `payment_ref` | `text` | NULLABLE | Payment reference ID (Payme/Click) |

---

## 2. Qo'shimcha (Helper Tables)

-   `skills` (skill_name): Standartlashtirilgan ko'nikmalar ro'yxati (AI uchun).
-   `cities` (city_name, region): O'zbekiston shaharlari ro'yxati.
-   `job_views` (job_id, user_id, viewed_at): Statistika uchun ko'rishlar tarixi.
-   `saved_jobs` (user_id, job_id, saved_at): Saqlangan ishlar.

---

## 3. Indekslar va Optimizatsiya

-   **Users**: `telegram_id` bo'yicha `UNIQUE INDEX`.
-   **Jobs**:
    -   `title` va `description` bo'yicha `GIN INDEX` (Full-text search uchun).
    -   `salary_min`, `salary_max`, `location`, `work_type` bo'yicha filterlar uchun oddiy indekslar (`BTREE`).
    -   `created_at` bo'yicha `DESC` tartiblash uchun.
-   **Applications**: `candidate_id` va `job_id` bo'yicha birgalikda indeks (`composite index`).

## 4. Enum Tiplari (PostgreSQL ENUM)

```sql
CREATE TYPE user_role AS ENUM ('candidate', 'recruiter', 'admin');
CREATE TYPE difficulty_level AS ENUM ('junior', 'middle', 'senior', 'lead', 'intern');
CREATE TYPE job_work_type AS ENUM ('remote', 'office', 'hybrid');
CREATE TYPE job_status AS ENUM ('active', 'closed', 'draft', 'archived');
CREATE TYPE app_status AS ENUM ('applied', 'viewed', 'interview', 'rejected', 'offer');
```
