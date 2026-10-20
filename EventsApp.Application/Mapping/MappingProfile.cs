using AutoMapper;
using EventsApp.Application.DTOs.Auth;
using EventsApp.Application.DTOs.Calendar;
using EventsApp.Application.DTOs.Candidacies;
using EventsApp.Application.DTOs.Categories;
using EventsApp.Application.DTOs.DeletionRequests;
using EventsApp.Application.DTOs.Events;
using EventsApp.Application.DTOs.Notifications;
using EventsApp.Application.DTOs.Providers;
using EventsApp.Application.DTOs.Users;
using EventsApp.Domain.Entities;

namespace EventsApp.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<RegisterRequestDto, User>()
            .ForMember(u => u.PasswordHash, o => o.Ignore())
            .ForMember(u => u.Role, o => o.Ignore());

        CreateMap<RegisterProviderRequestDto, User>()
            .ForMember(u => u.PasswordHash, o => o.Ignore())
            .ForMember(u => u.Role, o => o.Ignore());

        CreateMap<RegisterProviderRequestDto, Provider>()
            .ForMember(p => p.Description, o => o.MapFrom(d => d.Description ?? string.Empty))
            .ForMember(p => p.Status, o => o.Ignore())
            .ForMember(p => p.AcceptedTermsAt, o => o.Ignore());

        CreateMap<User, AuthResponseDto>()
            .ForMember(d => d.UserId, o => o.MapFrom(u => u.Id))
            .ForMember(d => d.Token, o => o.Ignore());

        CreateMap<Category, CategoryDto>();
        CreateMap<User, UserDto>();

        CreateMap<CompanyDay, CalendarDayDto>();                // Date ו-IsTaken זהים בשמם; Version לא נחשף

        CreateMap<CreateEventDto, Event>()                      // מטופס יצירה לישות
            .ForMember(e => e.Id, o => o.Ignore())              // נוצר ב-DB
            .ForMember(e => e.ClientId, o => o.Ignore())        // נקבע בשירות מהטוקן (אבטחה)
            .ForMember(e => e.Client, o => o.Ignore())          // לא ממפים ישויות מקושרות
            .ForMember(e => e.Day, o => o.Ignore())             // אותו עיקרון
            .ForMember(e => e.CreatedAtUtc, o => o.Ignore());   // נקבע בשירות

        CreateMap<CreateEventItemDto, EventItem>()              // מסעיף בטופס לישות
            .ForMember(i => i.Id, o => o.Ignore())              // נוצר ב-DB
            .ForMember(i => i.EventId, o => o.Ignore())         // EF ימלא לפי הקשר לאירוע
            .ForMember(i => i.Event, o => o.Ignore())           // לא ממפים ישויות מקושרות
            .ForMember(i => i.Category, o => o.Ignore())        // אותו עיקרון (CategoryId כן מועתק)
            .ForMember(i => i.ProviderId, o => o.Ignore())      // ספק נבחר רק על ידי האדמין, לא הלקוח
            .ForMember(i => i.Provider, o => o.Ignore())        // כנ"ל
            .ForMember(i => i.Candidacies, o => o.Ignore());    // אין מועמדויות ביצירה

        CreateMap<EventItem, EventItemDto>()                    // מישות ל-DTO
            .ForMember(d => d.CategoryName, o => o.MapFrom(i => i.Category.Name))     // שם הקטגוריה
            .ForMember(d => d.ProviderName, o => o.MapFrom(i => i.Provider != null ? i.Provider.BusinessName : null));   // שם העסק, או null אם אין ספק

        CreateMap<Event, EventDto>()                            // מישות ל-DTO
            .ForMember(d => d.ClientName, o => o.MapFrom(e => e.Client.FullName));    // שם הלקוח

        CreateMap<Provider, ProviderDto>()                      // מישות ל-DTO
            .ForMember(d => d.CategoryName, o => o.MapFrom(p => p.Category.Name))   // שם הקטגוריה
            .ForMember(d => d.Email, o => o.MapFrom(p => p.User.Email));            // אימייל מהמשתמש

        CreateMap<CreateCandidacyDto, Candidacy>()              // מטופס הגשה לישות
            .ForMember(c => c.Id, o => o.Ignore())              // נוצר ב-DB
            .ForMember(c => c.ProviderId, o => o.Ignore())      // נקבע בשירות מהטוקן
            .ForMember(c => c.Provider, o => o.Ignore())        // לא ממפים ישויות מקושרות
            .ForMember(c => c.EventItem, o => o.Ignore())       // אותו עיקרון (EventItemId כן מועתק)
            .ForMember(c => c.CreatedAtUtc, o => o.Ignore());   // נקבע בשירות

        CreateMap<Candidacy, CandidacyDto>()                    // מישות ל-DTO
            .ForMember(d => d.ProviderName, o => o.MapFrom(c => c.Provider.BusinessName))   // שם העסק
            .ForMember(d => d.Phone, o => o.MapFrom(c => c.Provider.Phone))                 // טלפון
            .ForMember(d => d.City, o => o.MapFrom(c => c.Provider.City));                  // עיר

        CreateMap<EventItem, OpenItemDto>()                     // סעיף ← תצוגה לספק
            .ForMember(d => d.ItemId, o => o.MapFrom(i => i.Id))                    // מזהה הסעיף
            .ForMember(d => d.EventDate, o => o.MapFrom(i => i.Event.Date))         // תאריך האירוע
            .ForMember(d => d.EventType, o => o.MapFrom(i => i.Event.Type))         // סוג האירוע
            .ForMember(d => d.Location, o => o.MapFrom(i => i.Event.Location))      // מקום
            .ForMember(d => d.GuestCount, o => o.MapFrom(i => i.Event.GuestCount))  // אורחים
            .ForMember(d => d.CategoryName, o => o.MapFrom(i => i.Category.Name))   // שם הקטגוריה
            .ForMember(d => d.AlreadyApplied, o => o.MapFrom(i => i.Candidacies.Any()));    // קיימת מועמדות של הספק (הטעינה סוננה בשאילתה)

        CreateMap<EventDeletionRequest, DeletionRequestDto>()   // מישות ל-DTO
            .ForMember(d => d.EventDate, o => o.MapFrom(r => r.Event.Date))             // תאריך האירוע
            .ForMember(d => d.EventType, o => o.MapFrom(r => r.Event.Type))             // סוג האירוע
            .ForMember(d => d.Location, o => o.MapFrom(r => r.Event.Location))          // מקום
            .ForMember(d => d.ClientId, o => o.MapFrom(r => r.Event.ClientId))          // מזהה הלקוח
            .ForMember(d => d.ClientName, o => o.MapFrom(r => r.Event.Client.FullName));    // שם הלקוח

        CreateMap<Candidacy, MyCandidacyDto>()                  // מועמדות ← תצוגה לספק
            .ForMember(d => d.EventDate, o => o.MapFrom(c => c.EventItem.Event.Date))       // תאריך
            .ForMember(d => d.EventType, o => o.MapFrom(c => c.EventItem.Event.Type))       // סוג
            .ForMember(d => d.Location, o => o.MapFrom(c => c.EventItem.Event.Location))    // מקום
            .ForMember(d => d.CategoryName, o => o.MapFrom(c => c.EventItem.Category.Name)) // קטגוריה
            .ForMember(d => d.Outcome, o => o.MapFrom(c =>                                  // תוצאה מחושבת:
                c.EventItem.ProviderId == null ? CandidacyOutcome.Open                      // אין ספק בסעיף: פתוח
                : (c.EventItem.ProviderId == c.ProviderId ? CandidacyOutcome.Won            // הספק הוא אני: זכיתי
                : CandidacyOutcome.Lost)));                                                 // ספק אחר: הפסדתי

        CreateMap<EventItem, BookingDto>()                      // סעיף משובץ ← תצוגה לספק
            .ForMember(d => d.ItemId, o => o.MapFrom(i => i.Id))                            // מזהה הסעיף
            .ForMember(d => d.EventDate, o => o.MapFrom(i => i.Event.Date))                 // תאריך
            .ForMember(d => d.EventType, o => o.MapFrom(i => i.Event.Type))                 // סוג
            .ForMember(d => d.Location, o => o.MapFrom(i => i.Event.Location))              // מקום
            .ForMember(d => d.GuestCount, o => o.MapFrom(i => i.Event.GuestCount))          // אורחים
            .ForMember(d => d.CategoryName, o => o.MapFrom(i => i.Category.Name))           // קטגוריה
            .ForMember(d => d.ClientName, o => o.MapFrom(i => i.Event.Client.FullName))     // שם לקוח
            .ForMember(d => d.ClientEmail, o => o.MapFrom(i => i.Event.Client.Email));      // אימייל לקוח

        CreateMap<Notification, NotificationDto>();             // שמות זהים, בלי הגדרות נוספות
    }
}
