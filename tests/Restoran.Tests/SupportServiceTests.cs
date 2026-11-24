using Moq;
using Restoran.Application.Common;
using Restoran.Application.DTOs;
using Restoran.Application.Interfaces;
using Restoran.Application.Services;

namespace Restoran.Tests;

/// <summary>Klienditoe vestlus: külaline ilma kontota ja reaalajas teated.</summary>
public class SupportServiceTests
{
    private readonly Mock<ISupportRepository> _messages = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<INotificationService> _notifications = new();
    private readonly Mock<ICurrentUser> _current = new();
    private readonly Mock<IClock> _clock = new();
    private readonly Mock<IRealtimeNotifier> _realtime = new();
    private static readonly Guid GuestKey = Guid.Parse("1b4e28ba-2fa1-11d2-883f-0016d3cca427");

    public SupportServiceTests()
    {
        _clock.Setup(c => c.Now).Returns(new DateTime(2026, 11, 23, 12, 0, 0));
        _current.Setup(c => c.UserId).Returns((int?)null); // sisse logimata
    }

    private SupportService Service() =>
        new(_messages.Object, _users.Object, _notifications.Object, _current.Object, _clock.Object, _realtime.Object);

    [Fact]
    public async Task Guest_CanWriteWithoutAccount_AndMessageIsSentLive()
    {
        var result = await Service().SendGuestAsync(GuestKey, new NewSupportMessageDto { Name = "Kati", Text = "Kas saab tulla koeraga?" });

        Assert.Equal("Kati", result.AuthorName);
        // Sõnum salvestatakse ilma kontota, ainult vestluse võtmega
        _messages.Verify(m => m.AddAsync(It.Is<SupportMessage>(x => x.UserId == null && x.GuestKey == GuestKey.ToString("N") && !x.FromStaff)), Times.Once);
        // ...ja jõuab kohe personalile ning sellele külalisele (WebSocket)
        _realtime.Verify(r => r.SupportMessageAsync(null, GuestKey.ToString("N"), It.IsAny<SupportMessageDto>()), Times.Once);
    }

    [Fact]
    public async Task StaffReply_ToGuest_GoesLiveWithoutNotification_AndUnknownConversationIs404()
    {
        _messages.Setup(m => m.GetGuestThreadAsync(It.IsAny<string>())).ReturnsAsync(new List<SupportMessage>());
        _messages.Setup(m => m.GetGuestThreadAsync(GuestKey.ToString("N")))
            .ReturnsAsync(new List<SupportMessage> { new() { Id = 1, GuestKey = GuestKey.ToString("N"), AuthorName = "Kati", Text = "Tere" } });

        await Service().ReplyGuestAsync(GuestKey, new NewSupportMessageDto { Text = "Tere, Kati!" });

        _realtime.Verify(r => r.SupportMessageAsync(null, GuestKey.ToString("N"), It.Is<SupportMessageDto>(d => d.FromStaff)), Times.Once);
        // Külalisel pole kontot, seega teadet (kellukest) ei looda
        _notifications.Verify(n => n.CreateAsync(It.IsAny<CreateNotificationDto>()), Times.Never);

        // Olematule vestlusele vastata ei saa
        await Assert.ThrowsAsync<NotFoundException>(() => Service().ReplyGuestAsync(Guid.NewGuid(), new NewSupportMessageDto { Text = "?" }));
    }
}
