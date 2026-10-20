using AutoMapper;
using EventsApp.Application.Abstractions;
using EventsApp.Application.DTOs.Providers;
using EventsApp.Application.Exceptions;
using EventsApp.Application.Repositories;
using EventsApp.Domain.Enums;

namespace EventsApp.Application.Services;

public class ProviderService : IProviderService
{
    private readonly IProviderRepository _providers;    // ספקים
    private readonly IEventItemRepository _items;       // חדש: סעיפים (לשיבוצים)
    private readonly IUnitOfWork _uow;                  // שמירה
    private readonly IMapper _mapper;                   // המרה

    public ProviderService(IProviderRepository providers, IEventItemRepository items,
        IUnitOfWork uow, IMapper mapper)            // הזרקה (תלות חדשה)
    {
        _providers = providers;
        _items = items;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<List<ProviderDto>> GetAsync(ProviderStatus? status, CancellationToken ct)
    {
        var list = await _providers.GetListAsync(status, ct);           // שולף (אולי מסונן)
        return _mapper.Map<List<ProviderDto>>(list);                    // ממיר
    }

    public async Task<ProviderDto> UpdateStatusAsync(int id, UpdateProviderStatusDto dto, CancellationToken ct)
    {
        if (dto.Status == ProviderStatus.Pending)                       // אסור להחזיר לממתין
            throw new BadRequestException("לא ניתן להחזיר ספק לסטטוס ממתין");   // 400

        var provider = await _providers.GetByIdAsync(id, ct)            // מחפש (במעקב)
                       ?? throw new NotFoundException("הספק לא נמצא");  // 404

        provider.Status = dto.Status;                                   // מעדכן; EF מזהה את השינוי
        await _uow.SaveChangesAsync(ct);                                // שומר
        return _mapper.Map<ProviderDto>(provider);                      // מחזיר את הספק המעודכן
    }

    public async Task<ProviderDto> GetMineAsync(int userId, CancellationToken ct)
    {
        var provider = await _providers.GetByUserIdAsync(userId, ct)    // הספק של המשתמש מהטוקן (כל סטטוס)
                       ?? throw new NotFoundException("פרופיל ספק לא נמצא");    // 404
        return _mapper.Map<ProviderDto>(provider);                      // מחזיר, כולל הסטטוס
    }

    public async Task<ProviderDto> UpdateMineAsync(int userId, UpdateProviderProfileDto dto, CancellationToken ct)
    {
        var provider = await _providers.GetByUserIdAsync(userId, ct)    // הספק של המשתמש מהטוקן
                       ?? throw new NotFoundException("פרופיל ספק לא נמצא");    // 404

        provider.BusinessName = dto.BusinessName.Trim();                // שם עסק
        provider.Description = dto.Description?.Trim() ?? string.Empty; // תיאור (העמודה אינה מקבלת null)
        provider.Phone = dto.Phone.Trim();                              // טלפון
        provider.City = dto.City.Trim();                                // עיר
        provider.StartingPrice = dto.StartingPrice;                     // מחיר התחלתי
        // בכוונה לא נוגעים ב-CategoryId וב-Status

        await _uow.SaveChangesAsync(ct);                                // שומר
        return _mapper.Map<ProviderDto>(provider);                      // מחזיר את המעודכן
    }

    public async Task<List<BookingDto>> GetBookingsAsync(int userId, CancellationToken ct)
    {
        var provider = await _providers.GetByUserIdAsync(userId, ct)    // הספק מהטוקן
                       ?? throw new NotFoundException("פרופיל ספק לא נמצא");    // 404
        var items = await _items.GetBookedAsync(provider.Id, ct);       // הסעיפים ששובצו לו
        return _mapper.Map<List<BookingDto>>(items);                    // ממיר
    }
}
