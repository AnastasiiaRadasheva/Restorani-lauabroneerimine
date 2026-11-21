namespace Restoran.Application.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string PasswordHash { get; set; } = "";
    public UserRole Role { get; set; } = UserRole.Customer;
    /// <summary>Profiilipilt (väike JPEG data-URL kujul).</summary>
    public string? AvatarUrl { get; set; }
    /// <summary>Kas klient soovib SMS-e (vaikimisi jah).</summary>
    public bool SmsOptIn { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>Kas kasutaja on ise parooli loonud. Personali loodud kliendil alguses false.</summary>
    public bool PasswordSet { get; set; } = true;

    /// <summary>Saidi keel (et/ru/en) – teated, SMS-id ja e-kirjad saadetakse selles keeles.</summary>
    public string Language { get; set; } = "et";
    /// <summary>Ühekordne link parooli loomiseks (kehtib 7 päeva).</summary>
    public string? PasswordToken { get; set; }
    public DateTime? PasswordTokenExpires { get; set; }

    /// <summary>Uus parooli loomise link (vana kaotab kehtivuse).</summary>
    public void IssuePasswordToken(DateTime now)
    {
        PasswordToken = Guid.NewGuid().ToString("N");
        PasswordTokenExpires = now.AddDays(7);
    }

    /// <summary>Kas kliendil on kehtiv link, mida saata.</summary>
    public bool HasValidPasswordToken(DateTime now) =>
        !PasswordSet && PasswordToken is not null && PasswordTokenExpires > now;

    public List<Reservation> Reservations { get; set; } = new();
    public List<Notification> Notifications { get; set; } = new();
}
