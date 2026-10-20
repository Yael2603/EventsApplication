using EventsApp.Application.Abstractions;                   // IUnitOfWork
using EventsApp.Application.DTOs.Events;                    // DTO-ים של אירועים
using EventsApp.Application.Exceptions;                     // חריגות העסק
using EventsApp.Application.Repositories;                   // חוזי המאגרים
using EventsApp.Application.Services;                       // EventService
using EventsApp.Domain.Entities;                            // הישויות
using EventsApp.Domain.Enums;                               // EventType
using Moq;                                                  // ספריית ה-Mock

namespace EventsApp.Tests.Services;                         // בדיקות שכבת השירותים

public class EventServiceTests                              // בדיקות יחידה: כל התלויות מוחלפות ב-Mock, בלי בסיס נתונים
{
    private readonly Mock<IEventRepository> _events = new();            // מאגר אירועים מזויף
    private readonly Mock<ICalendarRepository> _calendar = new();       // מאגר לוח מזויף
    private readonly Mock<ICategoryRepository> _categories = new();     // מאגר קטגוריות מזויף
    private readonly Mock<IEventItemRepository> _items = new();         // מאגר סעיפים מזויף
    private readonly Mock<IProviderRepository> _providers = new();      // מאגר ספקים מזויף
    private readonly Mock<IUnitOfWork> _uow = new();                    // שמירה מזויפת (כדי לוודא האם נקראה)
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);   // היום, כמו בשירות

    private EventService Sut() => new(_events.Object, _calendar.Object, _categories.Object,
        _items.Object, _providers.Object, _uow.Object, TestMapper.Create());   // השירות האמיתי עם תלויות מזויפות

    private static CreateEventDto ValidDto(DateOnly? date = null) => new()      // בקשה תקינה לשימוש חוזר
    {
        Date = date ?? Today.AddDays(30),                   // בעוד חודש
        Type = EventType.Wedding,
        Location = "אולמי הגן",
        GuestCount = 150,
        Items = new List<CreateEventItemDto> { new() { CategoryId = 1, Budget = 5000 } },   // סעיף אחד
    };

    [Fact]
    public async Task Create_TodayOrPast_ThrowsBadRequest_AndNeverSaves()
    {
        var dto = ValidDto(Today);                          // היום אינו תאריך עתידי
        await Assert.ThrowsAsync<BadRequestException>(() => Sut().CreateAsync(1, dto, CancellationToken.None));   // 400
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);   // לא נשמר דבר
    }

    [Fact]
    public async Task Create_DuplicateCategory_ThrowsBadRequest()
    {
        var dto = ValidDto();
        dto.Items.Add(new CreateEventItemDto { CategoryId = 1, Budget = 100 });     // אותה קטגוריה פעמיים
        await Assert.ThrowsAsync<BadRequestException>(() => Sut().CreateAsync(1, dto, CancellationToken.None));
    }

    [Fact]
    public async Task Create_UnknownCategory_ThrowsBadRequest()
    {
        _categories.Setup(c => c.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);    // הקטגוריה לא קיימת
        await Assert.ThrowsAsync<BadRequestException>(() => Sut().CreateAsync(1, ValidDto(), CancellationToken.None));
    }

    [Fact]
    public async Task Create_DateNotInCalendar_ThrowsBadRequest()
    {
        _categories.Setup(c => c.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _calendar.Setup(c => c.GetForUpdateAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>())).ReturnsAsync((CompanyDay?)null);   // אין יום כזה בלוח
        await Assert.ThrowsAsync<BadRequestException>(() => Sut().CreateAsync(1, ValidDto(), CancellationToken.None));
    }

    [Fact]
    public async Task Create_DayAlreadyTaken_ThrowsConflict_AndNeverSaves()
    {
        var dto = ValidDto();
        _categories.Setup(c => c.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _calendar.Setup(c => c.GetForUpdateAsync(dto.Date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CompanyDay { Date = dto.Date, IsTaken = true, Version = Guid.NewGuid() });    // היום כבר תפוס
        await Assert.ThrowsAsync<ConflictException>(() => Sut().CreateAsync(1, dto, CancellationToken.None));  // 409
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);   // ההגנה הראשונה עצרה לפני השמירה
        _events.Verify(e => e.Add(It.IsAny<Event>()), Times.Never);                         // אירוע לא נוסף
        _uow.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);       // הטרנזקציה בוטלה
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);        // ולא אושרה
    }

    [Fact]
    public async Task Create_FreeDay_TakesDay_SetsOwnerFromToken_AndSavesOnce()
    {
        var dto = ValidDto();
        var day = new CompanyDay { Date = dto.Date, IsTaken = false, Version = Guid.NewGuid() };    // יום פנוי
        Event? added = null;                                // כאן ייתפס האירוע שנוסף
        _categories.Setup(c => c.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _calendar.Setup(c => c.GetForUpdateAsync(dto.Date, It.IsAny<CancellationToken>())).ReturnsAsync(day);
        _events.Setup(e => e.Add(It.IsAny<Event>())).Callback<Event>(e => added = e);       // לוכד את האירוע שנוסף
        _events.Setup(e => e.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Event                         // מה שהמאגר "טוען מחדש" אחרי השמירה
            {
                Id = 10, ClientId = 5, Client = new User { FullName = "דנה" },
                Date = dto.Date, Location = dto.Location, GuestCount = dto.GuestCount,
                Items = new List<EventItem>(),
            });

        var result = await Sut().CreateAsync(5, dto, CancellationToken.None);   // לקוח מספר 5 (מגיע מהטוקן)

        Assert.True(day.IsTaken);                           // היום נתפס
        Assert.NotNull(added);                              // אירוע נוסף
        Assert.Equal(5, added!.ClientId);                   // הבעלים נקבע מהטוקן, לא מהבקשה
        Assert.Equal(10, result.Id);                        // הוחזר ה-DTO
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);    // שמירה אחת בלבד (טרנזקציה אחת)
        _uow.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);   // נפתחה טרנזקציה מפורשת
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);             // והיא אושרה
        _uow.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);          // ולא בוטלה
    }

    [Fact]
    public async Task Update_ByNonOwner_ThrowsForbidden()
    {
        _events.Setup(e => e.GetForUpdateAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Event { Id = 7, ClientId = 1, Date = Today.AddDays(10), Items = new List<EventItem>() });   // בעלים: לקוח 1
        var dto = new UpdateEventDto { Type = EventType.Brit, Location = "x", GuestCount = 10 };
        await Assert.ThrowsAsync<ForbiddenException>(() => Sut().UpdateAsync(2, 7, dto, CancellationToken.None));   // לקוח 2: 403
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddItem_DuplicateCategory_ThrowsConflict()
    {
        _events.Setup(e => e.GetForUpdateAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Event
            {
                Id = 7, ClientId = 1, Date = Today.AddDays(10),
                Items = new List<EventItem> { new() { CategoryId = 3 } },       // כבר קיים סעיף בקטגוריה 3
            });
        _categories.Setup(c => c.ExistsAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var dto = new CreateEventItemDto { CategoryId = 3, Budget = 100 };
        await Assert.ThrowsAsync<ConflictException>(() => Sut().AddItemAsync(1, 7, dto, CancellationToken.None));   // 409
    }

    [Fact]
    public async Task RemoveItem_WithAssignedProvider_ThrowsConflict()
    {
        _items.Setup(i => i.GetByIdAsync(4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventItem { Id = 4, EventId = 7, ProviderId = 9, Event = new Event { ClientId = 1, Date = Today.AddDays(10) } });   // כבר שובץ ספק
        await Assert.ThrowsAsync<ConflictException>(() => Sut().RemoveItemAsync(1, 4, CancellationToken.None));
        _items.Verify(i => i.Remove(It.IsAny<EventItem>()), Times.Never);   // לא נמחק
    }

    [Fact]
    public async Task RemoveItem_LastItem_ThrowsBadRequest()
    {
        _items.Setup(i => i.GetByIdAsync(4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventItem { Id = 4, EventId = 7, Event = new Event { ClientId = 1, Date = Today.AddDays(10) } });
        _items.Setup(i => i.CountByEventAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(1);   // זה הסעיף היחיד
        await Assert.ThrowsAsync<BadRequestException>(() => Sut().RemoveItemAsync(1, 4, CancellationToken.None));
    }

    [Fact]
    public async Task GetPage_PassesPageToRepository_AndReturnsTotals()
    {
        _events.Setup(e => e.GetPageAsync(2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Event> { new() { Id = 11, Client = new User { FullName = "דנה" }, Items = new List<EventItem>() } });   // עמוד 2 מחזיר אירוע אחד
        _events.Setup(e => e.CountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(11);    // 11 אירועים בסך הכול

        var result = await Sut().GetPageAsync(new() { Page = 2, PageSize = 10 }, CancellationToken.None);

        Assert.Single(result.Items);                        // רק שורות העמוד
        Assert.Equal(2, result.Page);                       // העמוד שהתבקש
        Assert.Equal(11, result.TotalCount);                // הסך הכולל
        Assert.Equal(2, result.TotalPages);                 // 11 שורות / 10 בעמוד = 2 עמודים
        _events.Verify(e => e.GetPageAsync(2, 10, It.IsAny<CancellationToken>()), Times.Once);  // העימוד נעשה במאגר (בבסיס הנתונים)
    }
}
