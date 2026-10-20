using AutoMapper;
using EventsApp.Application.Abstractions;
using EventsApp.Application.DTOs.Candidacies;
using EventsApp.Application.DTOs.Events;
using EventsApp.Application.Exceptions;
using EventsApp.Application.Repositories;
using EventsApp.Domain.Entities;
using EventsApp.Domain.Enums;

namespace EventsApp.Application.Services;

public class CandidacyService : ICandidacyService
{
    private readonly IProviderRepository _providers;    // ספקים
    private readonly IEventItemRepository _items;       // סעיפים
    private readonly ICandidacyRepository _candidacies; // מועמדויות
    private readonly IUnitOfWork _uow;                  // שמירה
    private readonly IMapper _mapper;                   // המרה

    public CandidacyService(IProviderRepository providers, IEventItemRepository items,
        ICandidacyRepository candidacies, IUnitOfWork uow, IMapper mapper)  // הזרקה
    {
        _providers = providers;
        _items = items;
        _candidacies = candidacies;
        _uow = uow;
        _mapper = mapper;
    }

    private async Task<Provider> GetApprovedProviderAsync(int userId, CancellationToken ct)  // עזר פרטי: ספק מחובר ומאושר
    {
        var provider = await _providers.GetByUserIdAsync(userId, ct)    // הספק של המשתמש מהטוקן
                       ?? throw new NotFoundException("פרופיל ספק לא נמצא");   // 404
        if (provider.Status != ProviderStatus.Approved)                 // עדיין לא אושר או נדחה
            throw new ForbiddenException("הפעולה זמינה רק לספק שאושר על ידי המנהל");   // 403
        return provider;
    }

    public async Task<List<OpenItemDto>> GetOpenItemsAsync(int userId, CancellationToken ct)
    {
        var provider = await GetApprovedProviderAsync(userId, ct);      // בדיקת ספק מאושר
        var today = DateOnly.FromDateTime(DateTime.UtcNow);             // היום
        var items = await _items.GetOpenAsync(provider.CategoryId, provider.Id, today, ct);  // סעיפים פתוחים בקטגוריה שלו
        return _mapper.Map<List<OpenItemDto>>(items);                   // ממיר
    }

    public async Task<CandidacyDto> ApplyAsync(int userId, CreateCandidacyDto dto, CancellationToken ct)
    {
        var provider = await GetApprovedProviderAsync(userId, ct);      // ספק מאושר בלבד

        var item = await _items.GetByIdAsync(dto.EventItemId, ct)       // הסעיף המבוקש
                   ?? throw new NotFoundException("הסעיף לא נמצא");     // 404

        if (item.CategoryId != provider.CategoryId)                     // קטגוריה אחרת
            throw new BadRequestException("הסעיף אינו בקטגוריה של הספק");   // 400
        if (item.Event.Date <= DateOnly.FromDateTime(DateTime.UtcNow))  // אירוע שעבר
            throw new BadRequestException("האירוע כבר התקיים או מתקיים היום");  // 400
        if (item.ProviderId != null)                                    // כבר שובץ ספק
            throw new ConflictException("כבר נבחר ספק לסעיף זה");       // 409
        if (await _candidacies.ExistsAsync(item.Id, provider.Id, ct))   // כבר הגיש
            throw new ConflictException("כבר הוגשה מועמדות לסעיף זה");  // 409

        var candidacy = _mapper.Map<Candidacy>(dto);                    // יוצר מועמדות מה-DTO
        candidacy.ProviderId = provider.Id;                             // הספק מהטוקן, לא מהבקשה
        candidacy.Provider = provider;                                  // הקשר לספק (שכבר במעקב), לטובת המיפוי
        candidacy.CreatedAtUtc = DateTime.UtcNow;                       // זמן הגשה

        _candidacies.Add(candidacy);                                    // מסמן הוספה
        await _uow.SaveChangesAsync(ct);                                // שומר; הפרת האינדקס הייחודי ← 409
        return _mapper.Map<CandidacyDto>(candidacy);                    // מחזיר ל-DTO
    }

    public async Task<List<CandidacyDto>> GetByItemAsync(int eventItemId, CancellationToken ct)
    {
        _ = await _items.GetByIdAsync(eventItemId, ct)                  // בודק שהסעיף קיים (התוצאה עצמה לא נחוצה)
            ?? throw new NotFoundException("הסעיף לא נמצא");            // 404
        var list = await _candidacies.GetByItemAsync(eventItemId, ct);  // המועמדים
        return _mapper.Map<List<CandidacyDto>>(list);                   // ממיר
    }

    public async Task<List<MyCandidacyDto>> GetMineAsync(int userId, CancellationToken ct)
    {
        var provider = await _providers.GetByUserIdAsync(userId, ct)    // הספק מהטוקן (כל סטטוס)
                       ?? throw new NotFoundException("פרופיל ספק לא נמצא");    // 404
        var list = await _candidacies.GetByProviderAsync(provider.Id, ct);      // המועמדויות שלו
        return _mapper.Map<List<MyCandidacyDto>>(list);                 // ממיר (כולל חישוב Outcome)
    }

    public async Task WithdrawAsync(int userId, int candidacyId, CancellationToken ct)
    {
        var provider = await _providers.GetByUserIdAsync(userId, ct)    // הספק מהטוקן
                       ?? throw new NotFoundException("פרופיל ספק לא נמצא");    // 404
        var candidacy = await _candidacies.GetByIdAsync(candidacyId, ct)        // המועמדות (במעקב, עם הסעיף)
                        ?? throw new NotFoundException("המועמדות לא נמצאה");    // 404

        if (candidacy.ProviderId != provider.Id)                        // של ספק אחר
            throw new ForbiddenException("ניתן לבטל רק מועמדות שלך");   // 403
        if (candidacy.EventItem.ProviderId == provider.Id)              // כבר שובצת בסעיף
            throw new ConflictException("לא ניתן לבטל מועמדות לסעיף ששובצת בו");    // 409

        _candidacies.Remove(candidacy);                                 // מסמן מחיקה
        await _uow.SaveChangesAsync(ct);                                // שומר
    }
}
