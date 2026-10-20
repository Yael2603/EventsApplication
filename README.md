# EventsApp: מערכת להפקת אירועים

מערכת צד שרת (ASP.NET Core Web API, .NET 8) לחברת הפקת אירועים. **לקוח** מזמין אירוע ביום פנוי ומגדיר לכל קטגוריה (קייטרינג, צילום, מוזיקה ועוד) תקציב ופרטים. **ספקים** מגישים מועמדות לסעיפים בקטגוריה שלהם. **אדמין** מאשר ספקים, בוחר ספק לכל סעיף (גם כזה שלא הגיש מועמדות), ומטפל בבקשות מחיקת אירוע.

פרויקט גמר בקורס .NET.

- כתובת הפריסה (Render): https://eventsapplication.onrender.com
- בדיקת תקינות: `GET /api/health`
- תיעוד אינטראקטיבי (Swagger): `/swagger`

---

## תוכן עניינים
1. [תפקידים והרשאות](#תפקידים-והרשאות)
2. [כללים עסקיים עיקריים](#כללים-עסקיים-עיקריים)
3. [ארכיטקטורה](#ארכיטקטורה)
4. [טכנולוגיות](#טכנולוגיות)
5. [מודל נתונים](#מודל-נתונים)
6. [נקודות קצה](#נקודות-קצה)
7. [טיפול בשגיאות ומעקב בקשות](#טיפול-בשגיאות-ומעקב-בקשות)
8. [אבטחה והגדרות](#אבטחה-והגדרות)
9. [הרצה מקומית](#הרצה-מקומית)
10. [מיגרציות](#מיגרציות)
11. [פריסה ל-Render](#פריסה-ל-render)
12. [בדיקות](#בדיקות)
13. [הערות על תלויות](#הערות-על-תלויות)
14. [מצב הפרויקט](#מצב-הפרויקט)

---

## תפקידים והרשאות

| תפקיד | איך נוצר | מה יכול לעשות |
|---|---|---|
| **Client** (לקוח) | הרשמה עצמית (`/api/auth/register`) | לראות את לוח השנה, ליצור אירוע, לערוך אותו, לבקש מחיקה עם נימוק |
| **Provider** (ספק) | הרשמה עם אישור תנאים (`/api/auth/register-provider`), נשאר `Pending` עד אישור אדמין | לנהל פרופיל, לראות סעיפים פתוחים בקטגוריה שלו, להגיש מועמדות, לראות שיבוצים |
| **Admin** (אדמין) | **יחיד**, נוצר אוטומטית בהפעלה מהגדרות סודיות (`Seed`) | לאשר ספקים, לשבץ ספקים, לנהל קטגוריות, לאשר או לדחות בקשות מחיקה, לראות סטטיסטיקות |

ההרשאות נאכפות באמצעות JWT ו-`[Authorize(Roles = ...)]`. זהות המשתמש נלקחת **תמיד מהטוקן** ולא מגוף הבקשה.

---

## כללים עסקיים עיקריים

- **אירוע אחד ליום.** תאריך נתפס בלוח החברה (`CompanyDay`), ואי אפשר להזמין יום תפוס (409).
- **הגנה מפני הזמנה כפולה בו-זמנית** (ראו "מודל נתונים"): בדיקה בקוד, `Version` עם בדיקת מקביליות אופטימית, ואינדקס ייחודי על תאריך האירוע.
- **סעיף אחד לכל קטגוריה** באירוע, ולפחות סעיף אחד באירוע.
- **ספק מגיש מועמדות רק** כשהוא מאושר, לסעיף בקטגוריה שלו, באירוע עתידי, וכל עוד לא שובץ בסעיף ספק. מועמדות כפולה נחסמת (409).
- **שיבוץ על ידי האדמין:** ספק מאושר, באותה קטגוריה של הסעיף, באירוע עתידי. מותר לשבץ גם ספק שלא הגיש מועמדות.
- **מחיקת אירוע:** הלקוח מגיש בקשה עם נימוק (10 עד 1000 תווים), והאדמין מאשר או דוחה. אישור מוחק את האירוע ומשחרר את היום באותה טרנזקציה, ושולח התראה לכל ספק ששובץ.
- **קטגוריה** לא נמחקת כל עוד יש בה ספקים או סעיפי אירועים (409). שמות קטגוריות ייחודיים ללא תלות באותיות גדולות וקטנות.
- ספק אינו יכול לשנות את הקטגוריה שלו, ועדכון פרטים אינו משנה את סטטוס האישור.

---

## ארכיטקטורה

Clean Architecture: התלויות מצביעות פנימה בלבד.

```
EventsApp.Domain          ישויות ו-Enums (בלי תלות בשום דבר)
        ↑
EventsApp.Application     DTOs, שירותים (חוקים עסקיים), ממשקי מאגרים, חריגות,
        ↑                 הפשטות (IUnitOfWork, IPasswordHasher, ITokenGenerator), AutoMapper
EventsApp.Infrastructure  AppDbContext (EF Core), מאגרים, UnitOfWork, אבטחה (BCrypt, JWT),
        ↑                 Seed, מיגרציות
EventsApp.API             Controllers, Middleware, Program.cs (חיבור ה-DI וההגדרות)

EventsApp.Tests           בדיקות xUnit
```

זרימה של כל בקשה:

```
HTTP → Middleware (CorrelationId, Exception) → Controller (הרשאה, DTO)
     → Service (חוקים עסקיים) → Repository (שאילתות) → PostgreSQL
```

עקרונות:
- **DTO + AutoMapper:** ישויות ה-Domain אינן נחשפות ללקוח.
- **Repository + Unit of Work:** השירותים מדברים עם ממשקים בלבד. `IUnitOfWork` שומר כמה שינויים בטרנזקציה אחת, ומתרגם שגיאות DB (מקביליות, הפרת ייחודיות) לחריגות של המערכת.
- **async ו-`CancellationToken`** בכל שכבה.
- **הגדרות** נטענות דרך `IOptions` (`JwtSettings`, `SeedSettings`), ואין סודות בקוד או במאגר.

---

## טכנולוגיות

| תחום | בחירה |
|---|---|
| Framework | ASP.NET Core Web API, .NET 8 |
| מסד נתונים | PostgreSQL 16 (Docker מקומית) |
| ORM | Entity Framework Core 8 + Npgsql, Code-First |
| מיפוי | AutoMapper 12 |
| אימות | JWT Bearer (HS256), סיסמאות מוצפנות ב-BCrypt |
| תיעוד API | Swagger (Swashbuckle) עם תמיכה ב-Bearer |
| בדיקות | xUnit + Moq |
| לוגים | NLog |
| פריסה | Docker + Render |

---

## מודל נתונים

| ישות | תיאור |
|---|---|
| `User` | משתמש: שם, אימייל (ייחודי), סיסמה מוצפנת, תפקיד |
| `Provider` | פרופיל ספק (1:1 עם `User`): עסק, תיאור, טלפון, עיר, **קטגוריה אחת**, מחיר התחלתי, סטטוס (`Pending`, `Approved`, `Rejected`), מועד אישור תנאים |
| `Category` | קטגוריית שירות (שם ייחודי) |
| `CompanyDay` | יום בלוח החברה: תאריך (מפתח), `IsTaken`, `Version` (בדיקת מקביליות) |
| `Event` | אירוע של לקוח: תאריך (ייחודי, מפתח זר ל-`CompanyDay`), סוג (Enum), מקום, אורחים, הערות |
| `EventItem` | סעיף באירוע לפי קטגוריה: תקציב, פרטים, ספק משובץ (אופציונלי) |
| `Candidacy` | מועמדות של ספק לסעיף: הודעה, מחיר מוצע. ייחודית לזוג (סעיף, ספק) |
| `EventDeletionRequest` | בקשת מחיקת אירוע: נימוק, סטטוס (`Pending`, `Rejected`), הערת אדמין |
| `Notification` | התראה למשתמש: הודעה, נקראה או לא |

**קשרים:**
- **אחד לרבים:** `User → Events`, `Event → EventItems`, `Category → Providers`, `EventItem → Candidacies`, `User → Notifications`.
- **רבים לרבים:** `Provider ↔ EventItem` דרך `Candidacy` (ספק מגיש לכמה סעיפים, וסעיף מקבל כמה ספקים).
- מחיקת אירוע מוחקת בשרשרת את סעיפיו, מועמדויותיו ובקשותיו (`Cascade`). קשרים אחרים הם `Restrict`.

**מקביליות (משאב מוגבל, יום בלוח):**
1. השירות בודק `IsTaken` ומחזיר 409 אם היום תפוס.
2. לשדה `CompanyDay.Version` יש `[ConcurrencyCheck]`, והוא מוחלף בכל עדכון. אם שני לקוחות קראו את היום כפנוי, הראשון שומר והשני נכשל ב-`DbUpdateConcurrencyException` ← `ConcurrencyException` ← **409**.
3. אינדקס ייחודי על `Event.Date` הוא רשת ביטחון אחרונה (שגיאת PostgreSQL `23505` ← `ConflictException` ← **409**).

תפיסת היום ויצירת האירוע נשמרות ב-`SaveChanges` אחד, ולכן או שהכול נשמר או שאין שינוי.

---

## נקודות קצה

סה"כ 34. כל הנתיבים תחת `/api`.

### אימות ומשתמשים
| שיטה | נתיב | הרשאה | תיאור | סטטוס |
|---|---|---|---|---|
| POST | `/api/auth/register` | פתוח | הרשמת לקוח | נכתב |
| POST | `/api/auth/register-provider` | פתוח | הרשמת ספק (ממתין לאישור) | נכתב |
| POST | `/api/auth/login` | פתוח | התחברות, מחזיר JWT | נכתב |
| GET | `/api/users/me` | מחובר | פרטי המשתמש המחובר | נכתב |
| GET | `/api/categories` | פתוח | רשימת קטגוריות | נכתב |

### לוח שנה ואירועים
| שיטה | נתיב | הרשאה | תיאור | סטטוס |
|---|---|---|---|---|
| GET | `/api/calendar?from=&to=` | מחובר | ימים פנויים ותפוסים (ברירת מחדל: 60 יום קדימה) | נכתב |
| POST | `/api/events` | Client | יצירת אירוע ותפיסת יום | נכתב |
| GET | `/api/events/my` | Client | האירועים שלי | נכתב |
| GET | `/api/events/{id}` | הבעלים או Admin | פרטי אירוע | נכתב |
| GET | `/api/events?page=1&pageSize=10` | Admin | כל האירועים, בעימוד (Skip/Take בבסיס הנתונים) | נכתב |

### ספקים, מועמדויות ושיבוץ
| שיטה | נתיב | הרשאה | תיאור | סטטוס |
|---|---|---|---|---|
| GET | `/api/providers?status=` | Admin | רשימת ספקים לפי סטטוס | נכתב |
| PUT | `/api/providers/{id}/status` | Admin | אישור או דחייה | נכתב |
| GET | `/api/event-items/open` | Provider | סעיפים פתוחים בקטגוריה שלי | נכתב |
| POST | `/api/candidacies` | Provider | הגשת מועמדות | נכתב |
| GET | `/api/event-items/{itemId}/candidacies` | Admin | מועמדים לסעיף | נכתב |
| PUT | `/api/event-items/{itemId}/provider` | Admin | שיבוץ ספק בסעיף | נכתב |

### ניהול קטגוריות ופרופיל ספק
| שיטה | נתיב | הרשאה | תיאור | סטטוס |
|---|---|---|---|---|
| POST | `/api/categories` | Admin | הוספת קטגוריה | נכתב |
| PUT | `/api/categories/{id}` | Admin | שינוי שם | נכתב |
| DELETE | `/api/categories/{id}` | Admin | מחיקה (חסומה אם בשימוש) | נכתב |
| GET | `/api/providers/me` | Provider | הפרופיל שלי | נכתב |
| PUT | `/api/providers/me` | Provider | עדכון פרטי העסק | נכתב |

### מחיקת אירוע באישור אדמין
| שיטה | נתיב | הרשאה | תיאור | סטטוס |
|---|---|---|---|---|
| POST | `/api/events/{eventId}/deletion-request` | Client (הבעלים) | בקשת מחיקה עם נימוק | נכתב |
| GET | `/api/deletion-requests?status=` | Admin | בקשות לפי סטטוס | נכתב |
| PATCH | `/api/deletion-requests/{id}` | Admin | החלטה על הבקשה: `{ "decision": "Approve" }` מוחק את האירוע ומשחרר את היום (204); `{ "decision": "Reject", "adminNote": "..." }` דוחה (200) | נכתב |

### עריכה, מעקב והתראות
| שיטה | נתיב | הרשאה | תיאור | סטטוס |
|---|---|---|---|---|
| PUT | `/api/events/{id}` | Client (הבעלים) | עריכת פרטי אירוע (בלי תאריך) | נכתב |
| POST | `/api/events/{id}/items` | Client (הבעלים) | הוספת סעיף | נכתב |
| DELETE | `/api/event-items/{itemId}` | Client (הבעלים) | הסרת סעיף | נכתב |
| GET | `/api/candidacies/my` | Provider | המועמדויות שלי ותוצאתן | נכתב |
| DELETE | `/api/candidacies/{id}` | Provider | ביטול מועמדות | נכתב |
| GET | `/api/providers/me/bookings` | Provider | סעיפים ששובצו אליי | נכתב |
| GET | `/api/notifications` | מחובר | ההתראות שלי | נכתב |
| PATCH | `/api/notifications/{id}` | מחובר | עדכון מצב קריאה: `{ "isRead": true }` | נכתב |
| GET | `/api/admin/stats` | Admin | סטטיסטיקות כלליות | נכתב |

קודי תשובה נפוצים: `200` הצלחה, `201` נוצר, `204` הצלחה בלי גוף, `400` בקשה לא תקינה, `401` לא מחובר, `403` אין הרשאה, `404` לא נמצא, `409` התנגשות.

---

## טיפול בשגיאות ומעקב בקשות

- **`ExceptionMiddleware`** תופס חריגות מותאמות ומתרגם אותן ל-HTTP:
  `NotFoundException` ← 404, `BadRequestException` ← 400, `UnauthorizedException` ← 401, `ForbiddenException` ← 403, `ConflictException` ו-`ConcurrencyException` ← 409. כל חריגה אחרת ← 500 עם הודעה כללית (בלי פרטים פנימיים), והפרטים נרשמים ביומן.
- התשובה בפורמט **`ProblemDetails`** וכוללת `correlationId`.
- **`CorrelationIdMiddleware`** קורא או יוצר את הכותרת `X-Correlation-ID` (לאחר ניקוי תווים), מחזיר אותה בתשובה, ומוסיף אותה להיקף הלוגים, כך שאפשר לאתר את כל השורות של בקשה אחת.
- ולידציה אוטומטית של DTOs (`[ApiController]` ו-Data Annotations) מחזירה 400.

---

## אבטחה והגדרות

- סיסמאות מוצפנות ב-**BCrypt**.
- **JWT** (HS256) עם `NameIdentifier`, `Email`, `Role`. תוקף ברירת מחדל: 60 דקות.
- **אין סודות במאגר.** מקומית משתמשים ב-**User Secrets**, ובשרת במשתני סביבה. האפליקציה מסרבת לעלות אם `Jwt:Key` חסר או קצר מ-32 תווים.
- **CORS** מוגבל לכתובות שמוגדרות ב-`Cors:AllowedOrigins`.
- כל משתמש רואה רק את הנתונים שלו (בדיקת בעלות בשירותים), ופרטי הלקוח נחשפים לספק רק אחרי שיבוץ.
- מיפוי DTO מונע "Mass Assignment": שדות רגישים (`ClientId`, `ProviderId` בהגשה, `Status`, `Role`) נקבעים בשרת בלבד.

### מפתחות הגדרה

| מפתח | משמעות | איפה מוגדר |
|---|---|---|
| `Jwt:Key` | מפתח חתימה סודי (32 תווים ומעלה) | User Secrets / משתנה סביבה |
| `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpirationMinutes` | פרטי הטוקן | `appsettings.json` |
| `ConnectionStrings:DefaultConnection` | חיבור ל-PostgreSQL | User Secrets / משתנה סביבה |
| `Seed:AdminEmail`, `Seed:AdminPassword`, `Seed:AdminFullName` | האדמין היחיד | User Secrets / משתנה סביבה |
| `Cors:AllowedOrigins` | כתובות הקליינט המורשות | `appsettings.json` / משתנה סביבה |

בהפעלה ראשונה (ואחריה בכל הפעלה, בצורה בטוחה לחזרה) המערכת מחילה מיגרציות ומזינה: 8 קטגוריות התחלתיות, ימי לוח לשנה קדימה, והאדמין.

---

## הרצה מקומית

**דרישות (סביבת .NET 8):**

| רכיב | גרסה | בדיקה |
|---|---|---|
| .NET SDK | **8.0.x** (הפרויקט מוגדר ל-`net8.0` ב-`Directory.Build.props`) | `dotnet --list-sdks` (חייבת להופיע שורה שמתחילה ב-`8.0`) |
| כלי EF | **8.x** (תואם לחבילות EF Core 8) | `dotnet ef --version` |
| Docker Desktop | עדכני | `docker ps` |
| Node.js (לקליינט) | 18 ומעלה | `node -v` |

התקנת כלי EF בגרסה התואמת (פעם אחת): `dotnet tool install --global dotnet-ef --version 8.*`

> אם מותקן רק SDK חדש יותר (9 או 10), יש להתקין גם .NET 8 מ-https://dotnet.microsoft.com/download/dotnet/8.0. ללא סביבת הרצה של .NET 8 השרת לא יעלה, כי הפרויקט מכוון ל-`net8.0`. הדרך ההפוכה (שינוי `net8.0` ב-`Directory.Build.props`) דורשת לעדכן גם את חבילות `8.0.*` ולכן אינה מומלצת.

**1. מסד נתונים (PostgreSQL ב-Docker):**
```bash
docker run --name eventsapp-db -e POSTGRES_PASSWORD=<סיסמה> -e POSTGRES_DB=eventsapp -p 5432:5432 -v eventsapp-data:/var/lib/postgresql/data -d postgres:16
```
בהפעלות הבאות: `docker start eventsapp-db`.

**2. סודות מקומיים (פעם אחת):**
```bash
cd EventsApp.API
dotnet user-secrets set "Jwt:Key" "<מחרוזת אקראית של 32 תווים לפחות>"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=eventsapp;Username=postgres;Password=<סיסמה>"
dotnet user-secrets set "Seed:AdminEmail" "<אימייל האדמין>"
dotnet user-secrets set "Seed:AdminPassword" "<סיסמה חזקה>"
dotnet user-secrets set "Seed:AdminFullName" "<שם האדמין>"
cd ..
```
לבדיקה (בלי להדביק את הפלט בשום מקום): `dotnet user-secrets list --project EventsApp.API`.

**3. בנייה והרצה של השרת:**
```bash
dotnet restore
dotnet build
dotnet run --project EventsApp.API
```
השרת מאזין ל-`http://localhost:8080` (או לפורט שבמשתנה הסביבה `PORT`). Swagger זמין בנתיב `/swagger`, ובדיקת חיים ב-`/api/health`. המיגרציות מוחלות אוטומטית בהפעלה, וה-Seed יוצר קטגוריות, ימי לוח ואת האדמין.

**4. בדיקות:**
```bash
dotnet test
```

**5. הקליינט (ריפו נפרד):**
```bash
git clone https://github.com/Yael2603/EventsApp-Client.git
cd EventsApp-Client
npm install
npm run dev
```
האתר זמין ב-http://localhost:5173. כתובת ה-API נקבעת ב-`.env.development` (`VITE_API_URL=http://localhost:8080`), ויש להפעיל מחדש את `npm run dev` אחרי כל שינוי בקובץ.

**6. שימוש ב-Swagger עם טוקן:** `POST /api/auth/login`, להעתיק את `token`, ללחוץ **Authorize** ולהדביק אותו.

---

## מיגרציות

Code-First עם EF Core. הקבצים נמצאים ב-`EventsApp.Infrastructure/Migrations`.

| מיגרציה | תוכן |
|---|---|
| `Init` | משתמשים, ספקים, קטגוריות, לוח החברה, אירועים, סעיפים, מועמדויות |
| `AddDeletionRequestsAndNotifications` | בקשות מחיקת אירוע (כולל אינדקס חלקי: בקשה ממתינה אחת לאירוע) והתראות |

יצירת מיגרציה חדשה:
```bash
dotnet ef migrations add <שם> --project EventsApp.Infrastructure --startup-project EventsApp.API
```
החלה ידנית (לא נדרשת מקומית, כי ההפעלה מחילה אוטומטית): `dotnet ef database update` עם אותם פרמטרים.

---

## פריסה ל-Render

הפריסה נעשית מ-GitHub באמצעות `Dockerfile` שבשורש הפרויקט (build ב-`sdk:8.0`, הרצה ב-`aspnet:8.0`). האפליקציה מאזינה לפורט מהמשתנה `PORT` (ברירת מחדל 8080).

**משתני סביבה בשירות ב-Render** (שני קווים תחתונים מייצגים נקודתיים בהגדרות):

| משתנה | דוגמה למשמעות |
|---|---|
| `Jwt__Key` | מפתח JWT סודי (32 תווים ומעלה) |
| `ConnectionStrings__DefaultConnection` | מחרוזת חיבור ל-PostgreSQL מתארח |
| `Seed__AdminEmail`, `Seed__AdminPassword` | פרטי האדמין היחיד |
| `Cors__AllowedOrigins__0` | כתובת הקליינט המורשית |

בדיקת תקינות: `GET /api/health`. ללא `Jwt__Key` תקין האפליקציה לא תעלה (בכוונה), ולכן יש להגדיר אותו לפני הפריסה הראשונה.

---

## בדיקות

פרויקט `EventsApp.Tests` (xUnit + Moq) מתמקד בשכבת השירותים ובמקרי הקצה העסקיים:

- בדיקות יחידה לשירותים (Moq למאגרים): `EventServiceTests` ו-`CandidacyServiceTests`, כולל ולידציות, חוקי בעלות והרשאה, ותרחישי 400, 403 ו-409.
- **בדיקת משאב מוגבל** (`CompanyDayConcurrencyTests`, על SQLite בזיכרון): שני `DbContext` נפרדים טוענים את אותו `CompanyDay` ומנסים לתפוס אותו. הראשון מצליח, והשני נכשל ב-`DbUpdateConcurrencyException`, שמתורגם ל-**409** (`ConcurrencyException`).
- אינדקס ייחודי על `Event.Date` דוחה אירוע שני באותו תאריך, גם בלי בדיקת הגרסה.
- שני לקוחות מזמינים את אותו יום בזה אחר זה: השני מקבל `ConflictException`, ונשמר אירוע אחד בלבד.

הרצה:
```bash
dotnet test
```

---

## הערות על תלויות

- **AutoMapper 12.0.1** מדווחת עליה אזהרת אבטחה (`NU1903`, CVE-2026-32933). ההשפעה מתייחסת למיפויים עמוקים ורקורסיביים, ובפרויקט כל המיפויים שטוחים ופשוטים (DTO אל ישות ולהפך), ללא נתונים רקורסיביים מבקשות חיצוניות. הבחירה מתועדת במודע, ואפשר לשדרג בעתיד לגרסה מתוקנת.
- הגרסאות המדויקות נמצאות בקבצי ה-`csproj` של כל פרויקט.

---

## הקליינט (React)

ריפו נפרד: https://github.com/Yael2603/EventsApp-Client — אתר React בעיצוב "SHOW הפקות" (Vite, React Router, Axios), RTL בעברית.

- **דפי לקוח:** בחירת תאריך בלוח חי, יצירה ועריכת אירוע, סעיפים, בקשת מחיקה, התראות.
- **דפי ספק:** סעיפים פתוחים, הגשה וביטול מועמדות עם תוצאה, שיבוצים, פרופיל.
- **דפי מנהל:** לוח בקרה, אירועים ושיבוץ ספקים, אישור ספקים, קטגוריות, בקשות מחיקה.
- **אבטחה בקליינט:** הטוקן נשלח בכותרת מ-`api/client.js`, `ProtectedRoute` חוסם לפי תפקיד (נוחות בלבד, האכיפה האמיתית בשרת), ו-401 מנתק אוטומטית.
- **הרצה:** בריפו הקליינט, `npm install`, `npm run dev`, ואז http://localhost:5173. כתובת ה-API ב-`.env.development` (`VITE_API_URL`).
- **פריסה:** ב-Render כאתר סטטי (`npm run build`, תיקיית `dist`), עם `VITE_API_URL` ו-`Cors__AllowedOrigins__0` בשרת.

## כתובות באינטרנט

- **API (Render):** https://eventsapp-api-f0ph.onrender.com (בדיקת תקינות: `/api/health`, תיעוד: `/swagger`)
- **אתר (Render Static Site):** https://eventsapp-client.onrender.com
- **קוד מקור — שרת:** https://github.com/Yael2603/EventsApplication
- **קוד מקור — קליינט:** https://github.com/Yael2603/EventsApp-Client
- **הערה:** בשירות חינמי ב-Render השרת "נרדם" אחרי חוסר פעילות, והבקשה הראשונה עשויה להימשך כדקה. ברשתות עם סינון תוכן, כתובות `onrender.com` עלולות להיחסם.

## מצב הפרויקט

- **הושלם:** תשתית (ארכיטקטורה, DB, JWT, Middleware, Swagger, Docker, Seed), כל נקודות הקצה, NLog, קליינט React ופריסת ה-API ל-Render עם PostgreSQL מתארח.
- **נבדק:** 24 בדיקות אוטומטיות עוברות (`dotnet test`), כולל הזמנה כפולה ומרוץ בין בקשות במקביל (אחת מצליחה, השנייה 409). תהליכי לקוח וספק נבדקו מול מסד אמיתי.
- **תהליך מנהל** (אישור ספק, שיבוץ, אישור מחיקה): מכוסה ב-`scripts/smoke-test.ps1` ובבדיקות היחידה. הרצת הסקריפט המלא מול סביבה חיה תלויה ברשת שאינה חוסמת את הכתובת.
- **החלטת עיצוב ב-DTO:** ספק משובץ רואה את שם הלקוח ואת האימייל שלו, כדי שיוכל ליצור קשר לצורך ההפקה.
