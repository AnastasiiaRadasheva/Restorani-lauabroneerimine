using Moq;
using Restoran.Application.Common;
using Restoran.Application.DTOs;
using Restoran.Application.Entities;
using Restoran.Application.Interfaces;
using Restoran.Application.Services;

namespace Restoran.Tests;

/// <summary>Registreerimine: üks inimene – üks konto (sama telefoninumbriga teist kontot ei looda).</summary>
public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();

    private AuthService Service() => new(_users.Object, new Mock<IPasswordService>().Object, new Mock<ITokenService>().Object,
        new Mock<ICurrentUser>().Object, new Mock<IBookingMessenger>().Object);

    [Fact]
    public async Task Register_WithPhoneOfAnotherAccount_Throws409_AndCreatesNothing()
    {
        _users.Setup(u => u.GetByPhoneAsync("+372 5555 1234", null))
            .ReturnsAsync(new User { Id = 7, Name = "Kati Klient", Email = "klient@restoran.ee", Phone = "55551234" });

        await Assert.ThrowsAsync<ConflictException>(() => Service().RegisterAsync(new RegisterDto
        {
            Name = "Kati Teine", Email = "kati.teine@example.com", Phone = "+372 5555 1234", Password = "Salasona123!"
        }));
        _users.Verify(u => u.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Theory]
    [InlineData("+372 5555 1234", "55551234", true)]
    [InlineData("00372 5555-1234", "5555 1234", true)]
    [InlineData("+372 5555 1234", "+372 5555 1235", false)]
    [InlineData("", "", false)]
    public void PhoneNumbers_AreComparedByDigitsOnly(string a, string b, bool same)
    {
        Assert.Equal(same, PhoneNumber.Same(a, b));
    }
}
