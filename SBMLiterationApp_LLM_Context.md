# LLM Context: SBMLiterationApp (SIGMA)

---

## 0. Rules for Copilot (antigravity) — READ THIS FIRST

> **These rules are non-negotiable and apply in every session.**

### Rule 1 — Write Before You Build
Before implementing ANY change, Copilot MUST:
1. Write a design log file in `./DesignLogs/` following the naming convention `yyyyMMddHHmmss_snake_case_name.md`
2. Present the design log to the user (ryusu-id) and explicitly ask for approval
3. Wait for approval (e.g. "go ahead", "approved", "yes", "proceed") before touching any code

No code changes, no pull requests, no file modifications — until the design log is approved.

### Rule 2 — Significance Levels
| Level | Trigger | Required Sections |
|-------|---------|-------------------|
| 🟢 Minor | Small bug fix, copy change, style tweak | Summary, Files Affected |
| 🟡 Moderate | New endpoint, new component, schema change | Summary, Motivation, Design Decision, Files Affected |
| 🔴 Major | New feature, architectural change, new module | Summary, Motivation, Design Decision, Diagrams, Files Affected, Risks & Mitigations |

### Rule 3 — Diagrams for Major Changes
For Major significance, include at least one Mermaid diagram:
- **Class Diagram** — new/changed domain entities
- **Sequence Diagram** — new multi-step flows
- **Flowchart** — event chains or process flows
- **ER Diagram** — database schema changes

### Rule 4 — Update the Log Index
After every design log creation or modification, update the index table in `DesignLogs/README.md`.

### Rule 5 — Session Startup
At the start of every new session, read:
1. This file (`SBMLiterationApp_LLM_Context.md`) — full system context
2. `DesignLogs/README.md` — rules and log index
3. The most recent 3 design logs — current state of the system

> Full rules are in [`DesignLogs/README.md`](./DesignLogs/README.md)

---

## 1. Project Overview

**SBMLiterationApp** is a full-stack web application branded as **SIGMA** — a **reading progress tracking and leaderboard platform** designed for **students** (particularly university students in Indonesia).

- **Developer/Owner:** ryusu.id (under PureTCO)
- **Repository:** [ryusu-id/SBMLiterationApp](https://github.com/ryusu-id/SBMLiterationApp)
- **App Name (Branded):** SIGMA
- **Purpose:** Enable students to log and track their reading sessions across books and academic journal papers, earn experience points (XP), maintain daily reading streaks, participate in leaderboards, receive reading recommendations, and complete daily reading challenges with quizzes.
- **Target Users:** University students — specifically those from faculties with Program Study fields like *Manajemen* and *Kewirausahaan*, affiliated with campuses such as Ganesha, Jatinangor, and Cirebon (strongly suggesting an ITB/SBM-affiliated context).

---

## 2. Tech Stack

### Language Composition
| Language    | Share  |
|-------------|--------|
| C#          | 53.6%  |
| Vue         | 40.3%  |
| TypeScript  | 5.1%   |
| Other       | 1.0%   |

### Backend
- **Runtime/Framework:** ASP.NET Core (C#) with **FastEndpoints** (REPR pattern — each endpoint is its own class)
- **ORM:** Entity Framework Core with **PostgreSQL** (via Npgsql)
- **Identity:** ASP.NET Core Identity (`IdentityUser<int>`) — used for user storage and role management only, **not** for password-based login
- **Authentication:** **Google OAuth 2.0** (Authorization Code Flow) — the **only** login method. After OAuth, the backend issues its own JWT access token + refresh token pair
- **Session:** `AddDistributedMemoryCache` + `AddSession` used for OAuth state management (CSRF protection, 10-minute idle timeout)
- **File Storage:** MinIO (S3-compatible object storage)
- **Architecture Style:** Vertical Slice / Feature-based module structure
- **Domain Events:** Custom `DomainEventsDispatcher` + `IDomainEventHandler<T>` pattern
- **API Documentation:** FastEndpoints Swagger integration
- **External Integrations:**
  - **Google OAuth 2.0** — user authentication
  - **Google Books API** — book metadata/cover images
  - **CrossRef API** — academic journal metadata

### Frontend
- **Framework:** **Nuxt 3** (Vue 3 SSR/SPA hybrid)
- **UI Library:** **Nuxt UI** (`UButton`, `UModal`, `UForm`, `UDashboardGroup`, `UNavigationMenu`, `UAuthForm`, etc.)
- **State Management:** **Pinia** (via `defineStore`)
- **HTTP Client:** `$fetch` / `useFetch` wrappers with JWT auto-refresh logic
- **Rich Text / Markdown:** Custom `MarkdownEditor.vue` component with TipTap-like toolbar (bold, italic, lists, blockquote, code block, link, image upload)
- **Animations:** **Lottie** animations for gamification feedback (level up, streaks, book complete)
- **Carousel:** Swiper.js (used for reading recommendations list)
- **Background Effect:** Custom `StarryBackground` composable/component
- **Validation:** Zod schemas on forms

---

## 3. Repository Structure

```
SBMLiterationApp/
├── DesignLogs/                 # Chronological design decision logs (see rules in Section 0)
│   ├── README.md               # Index + Copilot rules
│   ├── 20260223000000_initial_system_architecture.md
│   └── 20260223000001_event_storming.md
│
├── SBMLiterationApp_LLM_Context.md   # This file
│
├── Backend/                    # ASP.NET Core API (C#)
│   ├── Program.cs              # App bootstrap, DI registration
│   ├── Data/
│   │   └── ApplicationDbContext.cs   # EF Core DbContext
│   ├── Features/
│   │   ├── Auth/               # Google OAuth, JWT, Refresh Token
│   │   ├── AdminModule/        # Admin-only user management endpoints
│   │   ├── ReadingResourceModule/    # Books, Journals, Reading Reports
│   │   ├── ReadingCategoryModule/    # Reading categories (admin-managed)
│   │   ├── ReadingRecommendationModule/  # Admin-curated book recommendations
│   │   ├── DailyReadsModule/   # Daily reading challenges + quizzes
│   │   ├── StreakModule/       # Reading streak tracking
│   │   ├── UserXpModule/       # XP/EXP system, leaderboard
│   │   ├── ActivityFeeds/      # Activity feed for users
│   │   ├── IntegrationModule/  # Google Books API + CrossRef API
│   │   ├── FileSystem/         # MinIO file upload
│   │   └── Data/               # DB migration endpoint
│   └── Migrations/             # EF Core migrations
│
└── Frontend/                   # Nuxt 3 application (Vue/TypeScript)
    └── app/
        ├── pages/              # File-based routing
        │   ├── index.vue       # Landing page
        │   ├── signin.vue      # Sign-in page (Google button only)
        │   ├── auth/
        │   │   └── callback.vue  # OAuth callback handler
        │   ├── onboarding.vue  # New user profile setup
        │   ├── dashboard.vue   # Participant main dashboard
        │   ├── profile.vue     # User profile & reading reports
        │   ├── leaderboard/    # Leaderboard view
        │   ├── reading/        # Reading resources (books/journals) CRUD
        │   └── admin/          # Admin panel pages
        ├── components/         # Reusable Vue components
        ├── layouts/            # Nuxt layouts (landing, default, admin)
        ├── composables/        # Vue composables (auth, lottie, reading report, etc.)
        ├── middleware/          # Route guards (auth, admin-only, participant-only)
        └── apis/               # API client utilities
```

---

## 4. Authentication Flow (Google OAuth 2.0)

There is **no password-based login**. The **only** authentication method is **Google OAuth 2.0**.

### Full Flow

```
1. User clicks "Sign in with Google" on /signin
        ↓
2. Frontend calls GET /api/auth/google/url
   Backend generates a Google OAuth Authorization URL with:
   - client_id (from config)
   - redirect_uri: <Origin>/auth/callback
   - response_type: code
   - scope: openid profile email
   - state: random GUID (CSRF protection)
   - access_type: offline, prompt: consent
        ↓
3. Frontend redirects browser to Google: https://accounts.google.com/o/oauth2/v2/auth?...
        ↓
4. User authenticates with Google
        ↓
5. Google redirects to <Origin>/auth/callback?code=...&state=...
        ↓
6. Frontend (auth/callback.vue) POSTs { code, state } to POST /api/auth/google/callback
        ↓
7. Backend (GoogleAuthCallbackEndpoint):
   a. Exchanges authorization code for Google access token
      (POST https://oauth2.googleapis.com/token)
   b. Fetches user info from Google
      (GET https://www.googleapis.com/oauth2/v2/userinfo)
      Returns: { sub, email, name, picture, given_name, family_name, email_verified }
   c. Find-or-create user in DB by email (via ASP.NET Identity UserManager)
      - New user: created with email as username, Fullname from Google, empty NIM/ProgramStudy/Faculty/GenerationYear, PictureUrl from Google
      - Existing user: PictureUrl and Fullname synced from Google if missing
   d. Associates Google login via UserLoginInfo("Google", userInfo.Sub)
   e. Checks if account is locked/disabled
   f. Generates internal JWT access token + refresh token pair
   g. Returns { accessToken, refreshToken }
        ↓
8. Frontend stores tokens in secure cookies, redirects to /onboarding
        ↓
9. If profile already complete → redirected to /dashboard
```

### Token Management (Post-Login)
- **Access Token:** JWT, stored in a cookie (7-day `maxAge`), contains claims: `sub` (userId), `email`, `jti`, `given_name`, `role`
- **Refresh Token:** Opaque random token (32 bytes), stored in DB linked to access token's JTI, stored in a cookie (30-day `maxAge`)
- **Token Rotation:** On 401, frontend automatically calls `POST /auth/refresh` with the current access + refresh tokens. A single in-flight refresh promise prevents duplicate refresh calls
- **Token Revocation:** On refresh, the old refresh token is deleted from DB and a new pair is issued
- **Account Lockout:** Locked accounts receive a `400` error with a descriptive message during the OAuth callback

### Google OAuth Scopes Requested
- `openid` — identity
- `profile` — name, picture, given_name, family_name
- `email` — email address

### Auth-Specific Endpoints
| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/auth/google/url` | Get the Google OAuth authorization URL |
| POST | `/api/auth/google/callback` | Exchange OAuth code for JWT tokens |
| POST | `/api/auth/refresh` | Refresh JWT access token |

---

## 5. Core Features

### 5.1 User Management & Roles
- **Two roles:** `admin` and participant (no explicit role = participant)
- **Onboarding flow:** New users (Google-created accounts with empty profile fields) fill in: Full Name, NIM, Study Program, Faculty, Generation Year before accessing the dashboard
- Admin can assign/revoke admin roles and disable/enable user accounts
- User profile includes: `Fullname`, `NIM`, `ProgramStudy`, `Faculty`, `GenerationYear`, `PictureUrl` (auto-synced from Google)

### 5.2 Reading Resources (Books & Journals)
- Participants can add **Books** and **Journal Papers** to their personal reading list
- Both types share the `mt_reading_resource` table via TPH discriminator (`BOOK` | `JOURNAL`)
- Each resource has: `Title`, `Authors`, `PublishYear`, `Page`, `ReadingCategory`, `CssClass`, `CoverImageUri`, `ISBN`, optional `ResourceLink`
- Resources support full CRUD for participants — a resource with existing reports **cannot** be deleted (`CanBeDeleted()` guard)

### 5.3 Reading Reports (Progress Tracking)
- A **Reading Report** tracks one reading session on a resource
- Tracks: `CurrentPage`, `MaxPage`, `TimeSpent`, `Insight` (reflection text, max 200 chars), `LastUpdated`
- Progress is **persisted locally** (localStorage) via the `usePersistedReadingReport` composable before being submitted to the backend
- localStorage state is cleared when the authenticated user changes (keyed by fullname)
- Submitting a report raises `ReadingReportCreatedEvent` and `BookCompletedEvent` → triggers XP and streak side effects (see Section 6)

### 5.4 Daily Reads (Daily Challenges)
- Admin creates **Daily Read** entries (articles/content in Markdown) with: `Title`, `CoverImg`, `Content`, `Date`, `Category`, `Exp`, `MinimalCorrectAnswer`
- Each Daily Read can have attached **Quiz Questions** with multiple-choice answers
- Participants read the content, then answer the quiz
- Passing the quiz raises `QuizAnsweredEvent` → triggers XP and streak side effects
- Quiz supports retry attempts (`RetrySeq`); only the latest attempt per question is counted

### 5.5 Streaks
- The system tracks **daily reading streaks** via `StreakLog` entries (one per user per day — idempotent)
- Streaks can be created by: submitting a reading report OR passing a daily read quiz
- API returns `CurrentStreakDays`, `TotalExp`, and `WeeklyStatus` (7-day Mon–Sun calendar)
- A **7-day streak bonus** of `2 XP` is awarded every 7-day milestone (7th, 14th, 21st day, etc.)

### 5.6 XP / Experience Points System
- **Event sourcing-like** approach: every XP gain is stored as an immutable `UserExpEvent`
- A periodic **snapshot** mechanism (`UserExpSnapshot`) avoids full table scans — snapshot every 7 events
- A PostgreSQL **view** (`user_exp_leaderboard`) efficiently computes total XP per user
- **XP constants:**
  - `0.1 XP` per page read (`READING_PER_PAGE`)
  - `10 XP` for passing a daily read quiz (`DAILY_READ_QUIZ_PASSED`)
  - `3 XP` for completing a book (`BOOK_COMPLETED`)
  - `2 XP` 7-day streak bonus (`STREAK_7_DAYS_BONUS`)
  - Book completion has a **7-day cooldown** (`BOOK_COMPLETION_COOLDOWN_DAYS`)
- **XP Event Types:** `ReadingExp`, `DailyReadsExp`, `StreakExp`, `BookCompleted`, `RecommendedBookCompleted`

> See [`DesignLogs/20260223000001_event_storming.md`](./DesignLogs/20260223000001_event_storming.md) for the full event chain diagram.

### 5.7 Leaderboard
- Paginated leaderboard endpoint (`GET /leaderboard`) returning ranked users by total XP
- Each entry shows: `Rank`, `Exp`, `Username`, `PictureUrl`

### 5.8 Reading Recommendations
- Admins create curated reading recommendations with: `Title`, `ISBN`, `ReadingCategory`, `Authors`, `PublishYear`, `Page`, `ResourceLink`, `CoverImageUri`, `Exp`
- Displayed on the participant dashboard as a Swiper carousel
- Completing a recommended book grants `RecommendedBookCompleted` XP event

### 5.9 Reading Categories
- Admin-managed taxonomy for categorizing reading resources

### 5.10 Activity Feeds
- Paginated feed of user activity (reading sessions, daily reads completed) with natural-language descriptions generated from `UserExpEvent.EventName`

---

## 6. Domain Event Map (Summary)

> Full event storming diagram: [`DesignLogs/20260223000001_event_storming.md`](./DesignLogs/20260223000001_event_storming.md)

| Domain Event | Raised By | Key Side Effects |
|-------------|-----------|-----------------|
| `ReadingReportCreatedEvent` | `ReadingReport.Create()` | XP (page × 0.1) + StreakLog for today |
| `BookCompletedEvent` | `ReadingReport.Create()` | XP (3, 7-day cooldown) |
| `StreakLogCreatedEvent` | `StreakLog.Create()` | XP (2) if streak % 7 == 0 |
| `QuizAnsweredEvent` | Quiz answer endpoint | XP (10) if passing + StreakLog for today |
| `UserExpCreatedEvent` | `UserExpEvent.Create()` | Snapshot every 7 events |
| `ReadingBookCreatedEvent` | `Book.Create()` | No handlers yet |
| `ReadingRecommendationCreatedEvent` | `ReadingRecommendation.Create()` | No handlers yet |

---

## 7. Frontend Design

### Layouts
| Layout | Used For |
|--------|---------|
| `landing` | Home/index, sign-in, OAuth callback, onboarding, legal pages |
| `default` | Participant-facing pages (dashboard, profile, reading, leaderboard) |
| `admin` | Admin panel with collapsible sidebar |

### Navigation & Route Guards
- `auth` middleware — redirects unauthenticated users to `/signin`
- `admin-only` middleware — redirects non-admins to `/dashboard`
- `participant-only` middleware — redirects admins to `/admin`
- Login auto-redirects: admins → `/admin`, participants → `/dashboard`
- Authenticated users visiting `/` are redirected to `/dashboard`

### Admin Panel Pages
- `/admin` — Admin home
- `/admin/categories` — Manage reading categories
- `/admin/recommendation` — Manage reading recommendations
- `/admin/daily-reads` — CRUD for daily reads (with quiz management)
- `/admin/users` — User management (query, assign/revoke admin, disable/enable)

### Participant Pages
- `/dashboard` — Main hub: streak widget, tabs for reading resources, recommendations, daily reads, reading reports in progress
- `/profile` — User profile editing + in-progress reading reports list
- `/reading/books/create` & `/reading/journals/create` — Add new reading resources
- `/leaderboard` — View leaderboard
- `/auth/callback` — Google OAuth redirect handler

### Gamification UI
- Lottie animation overlays: level up, streak milestone, book completion
- Starry animated background (togglable)

---

## 8. Backend Architecture Patterns

### Feature Module Pattern (Vertical Slices)
Each feature is self-contained under `Backend/Features/<FeatureName>/`:
- `Domain/` — Entity classes, value objects, domain events
- `Endpoints/` — FastEndpoints classes (each endpoint = one class)
- `EndpointGroup` — Groups endpoints under a common route prefix + auth policy

### Domain Events
- Entities raise domain events via `.Raise()` on the `AuditableEntity` base
- `DomainEventsDispatcher` dispatches them inside `UnitOfWork.SaveChangesAsync()`

### API Response Convention
- All endpoints return `ApiResponse<T>` or `ApiResponse`
- Paging uses `PagingQuery` request + `PagingResult<T>` response

### Endpoint Groups (Route Prefixes)
| Group | Prefix | Auth |
|-------|--------|------|
| `GlobalApiEndpointGroup` | `/api` | — |
| `ReadingResourceEndpointGroup` | `/api/reading-resources` | Bearer JWT |
| `AdminEndpointGroup` | `/api/users` | Role: `admin` |
| `StreakEndpointGroup` | `/api/streaks` | — |
| `ReadingCategoryEndpointGroup` | `/api/reading-categories` | — |
| Reading Recommendations | `/api/reading-recommendations` | — |
| `DailyReadsModule` | `/api/daily-reads` | — |

---

## 9. Database Entities (EF Core DbSets)

> Full class diagram: [`DesignLogs/20260223000000_initial_system_architecture.md`](./DesignLogs/20260223000000_initial_system_architecture.md)

| Entity | Purpose |
|--------|---------|
| `User` | Extended IdentityUser — Google OAuth only, no password |
| `RefreshToken` | JWT refresh token rotation store (linked to JTI) |
| `Book` | Book reading resource (TPH: `BOOK`) |
| `JournalPaper` | Academic journal resource (TPH: `JOURNAL`) |
| `ReadingReport` | Per-user reading session progress |
| `StreakLog` | Daily streak tracking records |
| `ReadingCategory` | Admin-managed reading categories |
| `ReadingRecommendation` | Curated book recommendations |
| `DailyRead` | Daily reading challenge content |
| `QuizQuestion` | Quiz questions for a DailyRead |
| `QuizChoice` | Multiple-choice options for a QuizQuestion |
| `QuizAnswer` | User quiz answer (with retry seq) |
| `UserExpEvent` | Append-only XP event log |
| `UserExpSnapshot` | Periodic XP aggregate snapshot |
| `UserExpLeaderboard` | PostgreSQL view for leaderboard queries |

---

## 10. Key Business Rules

1. **Authentication is exclusively via Google OAuth 2.0.** No email/password login exists. ASP.NET Identity is used only for user storage and role management.
2. **New users** are auto-created on first Google login with empty `NIM`, `ProgramStudy`, `Faculty`, `GenerationYear`. They are redirected to `/onboarding` to complete their profile.
3. **Profile picture and fullname** are synced from Google on every login if missing locally.
4. **Participants** can only manage their own reading resources and reports. **Admins** manage categories, recommendations, and daily reads.
5. **XP is immutable and append-only** — no XP record is ever deleted.
6. **Book completion** has a **7-day cooldown** to prevent XP farming.
7. **Quiz retries** are allowed; only the latest attempt per question counts toward the score.
8. **Streak creation is idempotent** — only one `StreakLog` per user per day, regardless of how many reading reports or quiz passes occur.
9. **Streak XP bonus** is awarded on every multiple of 7 consecutive days (7th, 14th, 21st, etc.), with an idempotency check on `StreakLog.Id`.
10. **NIM** is a required profile field collected at onboarding for academic identification.
11. **Reading resources with existing reports cannot be deleted** — enforced by `CanBeDeleted()` on `ReadingResourceBase`.
12. **Token refresh** is handled transparently client-side; if a refresh is in-flight, duplicate requests wait on the same promise.
13. **Locked accounts** are rejected at the OAuth callback stage via `IsLockedOutAsync()` — they never receive a JWT.

---

## 11. Legal & Branding

- App is branded **SIGMA**, developed by **ryusu.id** under **PureTCO**
- Company contact: info@pure-tco.com, Green Lake City, Cipondoh, Tangerang, Indonesia
- The app has a **Privacy Policy** and **Terms of Service** built into the frontend
- Google OAuth is the only auth method; scopes: `openid profile email`
- Users can revoke SIGMA's Google access at https://myaccount.google.com/permissions
- Student data (NIM, faculty, program study, generation year) is collected for academic organization
- Profile picture, name, and leaderboard rank are **visible to other users**
- All other data is restricted to the individual user and admins