using EventsApp.Application.Exceptions;                     // ConcurrencyException, ConflictException
using EventsApp.Application.DTOs.Events;                    // DTO ליצירת אירוע
using EventsApp.Application.Services;                       // EventService
using EventsApp.Domain.Entities;                            // הישויות
using EventsApp.Domain.Enums;                               // Enums
using EventsApp.Infrastructure.Data;                        // AppDbContext, UnitOfWork
using EventsApp.Infrastructure.Repositories;                // המאגרים האמיתיים
using Microsoft.Data.Sqlite;                                // חיבור SQLite
using Microsoft.EntityFrameworkCore;                        // EF Core

namespace EventsApp.Tests.Concurrency;                      // בדיקות "משאב מוגבל": יום אחד בלוח, מזמין אחד בלבד

// בסיס נתונים אמיתי בזיכרון (SQLite) שמשותף לשני DbContext נפרדים. כך מדמים שתי בקשות במקביל:
// כל בקשה בשרת מקבלת DbContext משלה, ושתיהן קוראות לפני שמישהי שמרה.
public class CompanyDayConcurrencyTests : IDisposable
{
    private static readonly DateOnly TestDate = new(2030, 1, 1);    // היום שעליו "נלחמים"
    private readonly SqliteConnection _connection;                  // חיבור אחד פתוח (אחרת בסיס הנתונים בזיכרון נעלם)
    private readonly DbContextOptions<AppDbContext> _options;       // הגדרות משותפות לשני ההקשרים

    public CompanyDayConcurrencyTests()                             // רץ לפני כל בדיקה: בסיס נתונים נקי
    {
        _connection = new SqliteConnection("DataSource=:memory:");  // בסיס נתונים בזיכרון
        _connection.Open();                                         // חייב להישאר פתוח לאורך הבדיקה
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;

        using var ctx = new AppDbContext(_options);                 // הקשר זמני להכנת הנתונים
        ctx.Database.EnsureCreated();                               // יוצר את הטבלאות מהמודל
        ctx.CompanyDays.Add(new CompanyDay { Date = TestDate, IsTaken = false, Version = Guid.NewGuid() });    // יום פנוי
        ctx.Categories.Add(new Category { Name = "מוזיקה" });       // קטגוריה לסעיף (Id = 1)
        ctx.Users.Add(new User { FullName = "לקוח א", Email = "a@test.com", PasswordHash = "x", Role = UserRole.Client, CreatedAtUtc = DateTime.UtcNow });   // לקוח 1
        ctx.Users.Add(new User { FullName = "לקוח ב", Email = "b@test.com", PasswordHash = "x", Role = UserRole.Client, CreatedAtUtc = DateTime.UtcNow });   // לקוח 2
        ctx.SaveChanges();                                          // שומר
    }

    public void Dispose() => _connection.Dispose();                 // סוגר את בסיס הנתונים אחרי הבדיקה

    private AppDbContext NewContext() => new(_options);             // "בקשה" חדשה = הקשר חדש

    // ----- שכבה 2: Version (אופטימיסטי) -----

    [Fact]
    public async Task TwoContexts_SameDay_SecondSaveThrowsDbUpdateConcurrencyException()
    {
        await using var a = NewContext();                           // בקשה A
        await using var b = NewContext();                           // בקשה B
        var dayA = await a.CompanyDays.SingleAsync(d => d.Date == TestDate);    // A קוראת: פנוי, גרסה v1
        var dayB = await b.CompanyDays.SingleAsync(d => d.Date == TestDate);    // B קוראת: פנוי, גרסה v1

        dayA.IsTaken = true;                                        // A תופסת את היום
        await a.SaveChangesAsync();                                 // A שומרת ראשונה: מצליחה, הגרסה מוחלפת ל-v2

        dayB.IsTaken = true;                                        // B גם תופסת (היא עדיין חושבת שהיום פנוי)
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => b.SaveChangesAsync());     // ה-UPDATE של B לא מוצא Version=v1
    }

    [Fact]
    public async Task UnitOfWork_TranslatesConcurrencyFailure_To_ConcurrencyException()
    {
        await using var a = NewContext();
        await using var b = NewContext();
        var dayA = await a.CompanyDays.SingleAsync(d => d.Date == TestDate);
        var dayB = await b.CompanyDays.SingleAsync(d => d.Date == TestDate);
        dayA.IsTaken = true;
        await new UnitOfWork(a).SaveChangesAsync(CancellationToken.None);       // A שומרת דרך ה-UnitOfWork

        dayB.IsTaken = true;
        await Assert.ThrowsAsync<ConcurrencyException>(() => new UnitOfWork(b).SaveChangesAsync(CancellationToken.None));   // זה מה שה-Middleware הופך ל-409
    }

    [Fact]
    public async Task Version_ChangesOnEveryUpdate()
    {
        await using var ctx = NewContext();
        var day = await ctx.CompanyDays.SingleAsync(d => d.Date == TestDate);
        var before = day.Version;                                   // הגרסה המקורית
        day.IsTaken = true;
        await ctx.SaveChangesAsync();                               // ה-AppDbContext מחליף Version ידנית (ב-PostgreSQL אין rowversion)
        Assert.NotEqual(before, day.Version);                       // הגרסה השתנתה
    }

    [Fact]
    public async Task OnlyFirstWriterWins_DayEndsTakenExactlyOnce()
    {
        await using var a = NewContext();
        await using var b = NewContext();
        var dayA = await a.CompanyDays.SingleAsync(d => d.Date == TestDate);
        var dayB = await b.CompanyDays.SingleAsync(d => d.Date == TestDate);
        dayA.IsTaken = true; await a.SaveChangesAsync();
        dayB.IsTaken = true;
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => b.SaveChangesAsync());

        await using var check = NewContext();                       // הקשר חדש לבדיקת המצב האמיתי ב-DB
        Assert.True((await check.CompanyDays.SingleAsync(d => d.Date == TestDate)).IsTaken);    // היום תפוס
    }

    // ----- שכבה 3: אינדקס ייחודי על Event.Date -----

    private static Event NewEvent(int clientId) => new()
    {
        ClientId = clientId, Date = TestDate, Type = EventType.Wedding,
        Location = "אולם", GuestCount = 100, CreatedAtUtc = DateTime.UtcNow,
    };

    [Fact]
    public async Task UniqueIndex_RejectsSecondEventOnSameDate_EvenWithoutVersionCheck()
    {
        await using var a = NewContext();
        await using var b = NewContext();
        a.Events.Add(NewEvent(1));                                  // לקוח 1 מזמין
        b.Events.Add(NewEvent(2));                                  // לקוח 2 מזמין את אותו תאריך (בלי לגעת ב-CompanyDay בכלל)
        await a.SaveChangesAsync();                                 // הראשון נשמר
        await Assert.ThrowsAsync<DbUpdateException>(() => b.SaveChangesAsync());   // האינדקס הייחודי דוחה (ב-PostgreSQL: 23505 ו-ConflictException)
    }

    // ----- שכבה 1 מול בסיס נתונים אמיתי: הבקשה השנייה אחרי הראשונה -----

    private EventService NewService(AppDbContext ctx) => new(      // שירות אמיתי עם מאגרים אמיתיים, על הקשר נתון
        new EventRepository(ctx), new CalendarRepository(ctx), new CategoryRepository(ctx),
        new EventItemRepository(ctx), new ProviderRepository(ctx), new UnitOfWork(ctx), TestMapper.Create());

    private static CreateEventDto Dto() => new()
    {
        Date = TestDate, Type = EventType.Wedding, Location = "אולם", GuestCount = 100,
        Items = new List<CreateEventItemDto> { new() { CategoryId = 1, Budget = 3000 } },
    };

    [Fact]
    public async Task TwoClientsSameDay_SequentialRequests_SecondGetsConflict_OneEventSaved()
    {
        await using var a = NewContext();
        await using var b = NewContext();

        await NewService(a).CreateAsync(1, Dto(), CancellationToken.None);          // לקוח 1: מצליח
        await Assert.ThrowsAsync<ConflictException>(                                 // לקוח 2: "התאריך כבר תפוס" (409)
            () => NewService(b).CreateAsync(2, Dto(), CancellationToken.None));

        await using var check = NewContext();
        Assert.Equal(1, await check.Events.CountAsync(e => e.Date == TestDate));    // נשמר אירוע אחד בלבד
    }
}
