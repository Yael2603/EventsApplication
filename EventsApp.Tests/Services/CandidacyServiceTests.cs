using EventsApp.Application.Abstractions;
using EventsApp.Application.DTOs.Candidacies;
using EventsApp.Application.Exceptions;
using EventsApp.Application.Repositories;
using EventsApp.Application.Services;
using EventsApp.Domain.Entities;
using EventsApp.Domain.Enums;
using Moq;

namespace EventsApp.Tests.Services;

public class CandidacyServiceTests                          // בדיקות יחידה לכללי הגשת מועמדות
{
    private readonly Mock<IProviderRepository> _providers = new();
    private readonly Mock<IEventItemRepository> _items = new();
    private readonly Mock<ICandidacyRepository> _candidacies = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    private CandidacyService Sut() => new(_providers.Object, _items.Object, _candidacies.Object, _uow.Object, TestMapper.Create());

    private static Provider ApprovedProvider() => new()     // ספק מאושר בקטגוריה 2
    {
        Id = 20, UserId = 5, CategoryId = 2, Status = ProviderStatus.Approved, BusinessName = "דיג'יי הפקות",
    };

    private static EventItem OpenItem(int categoryId = 2) => new()      // סעיף פתוח באירוע עתידי
    {
        Id = 3, CategoryId = categoryId, Event = new Event { Date = Today.AddDays(20) },
    };

    private void GivenProvider(Provider p) =>               // הספק שמחובר
        _providers.Setup(r => r.GetByUserIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(p);

    private static CreateCandidacyDto Dto() => new() { EventItemId = 3, ProposedPrice = 4500 };

    [Fact]
    public async Task Apply_PendingProvider_ThrowsForbidden()
    {
        var p = ApprovedProvider(); p.Status = ProviderStatus.Pending;      // ספק שעוד לא אושר
        GivenProvider(p);
        await Assert.ThrowsAsync<ForbiddenException>(() => Sut().ApplyAsync(5, Dto(), CancellationToken.None));    // 403
    }

    [Fact]
    public async Task Apply_ItemInOtherCategory_ThrowsBadRequest()
    {
        GivenProvider(ApprovedProvider());
        _items.Setup(i => i.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(OpenItem(categoryId: 9));  // קטגוריה אחרת
        await Assert.ThrowsAsync<BadRequestException>(() => Sut().ApplyAsync(5, Dto(), CancellationToken.None));
    }

    [Fact]
    public async Task Apply_PastEvent_ThrowsBadRequest()
    {
        GivenProvider(ApprovedProvider());
        var item = OpenItem(); item.Event.Date = Today;     // האירוע היום
        _items.Setup(i => i.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(item);
        await Assert.ThrowsAsync<BadRequestException>(() => Sut().ApplyAsync(5, Dto(), CancellationToken.None));
    }

    [Fact]
    public async Task Apply_ItemAlreadyHasProvider_ThrowsConflict()
    {
        GivenProvider(ApprovedProvider());
        var item = OpenItem(); item.ProviderId = 77;        // כבר נבחר ספק
        _items.Setup(i => i.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(item);
        await Assert.ThrowsAsync<ConflictException>(() => Sut().ApplyAsync(5, Dto(), CancellationToken.None));
    }

    [Fact]
    public async Task Apply_DuplicateApplication_ThrowsConflict()
    {
        GivenProvider(ApprovedProvider());
        _items.Setup(i => i.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(OpenItem());
        _candidacies.Setup(c => c.ExistsAsync(3, 20, It.IsAny<CancellationToken>())).ReturnsAsync(true);    // כבר הוגשה
        await Assert.ThrowsAsync<ConflictException>(() => Sut().ApplyAsync(5, Dto(), CancellationToken.None));
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Apply_Valid_SetsProviderFromToken_AndSaves()
    {
        GivenProvider(ApprovedProvider());
        _items.Setup(i => i.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(OpenItem());
        Candidacy? added = null;
        _candidacies.Setup(c => c.Add(It.IsAny<Candidacy>())).Callback<Candidacy>(c => added = c);

        await Sut().ApplyAsync(5, Dto(), CancellationToken.None);

        Assert.NotNull(added);
        Assert.Equal(20, added!.ProviderId);                // הספק נקבע מהטוקן ולא מהבקשה
        Assert.Equal(4500, added.ProposedPrice);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Withdraw_OtherProvidersCandidacy_ThrowsForbidden()
    {
        GivenProvider(ApprovedProvider());
        _candidacies.Setup(c => c.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Candidacy { Id = 1, ProviderId = 99, EventItem = new EventItem() });  // מועמדות של ספק אחר
        await Assert.ThrowsAsync<ForbiddenException>(() => Sut().WithdrawAsync(5, 1, CancellationToken.None));
    }

    [Fact]
    public async Task Withdraw_WhenAlreadyChosen_ThrowsConflict()
    {
        GivenProvider(ApprovedProvider());
        _candidacies.Setup(c => c.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Candidacy { Id = 1, ProviderId = 20, EventItem = new EventItem { ProviderId = 20 } });    // הספק כבר שובץ
        await Assert.ThrowsAsync<ConflictException>(() => Sut().WithdrawAsync(5, 1, CancellationToken.None));
        _candidacies.Verify(c => c.Remove(It.IsAny<Candidacy>()), Times.Never);
    }
}
