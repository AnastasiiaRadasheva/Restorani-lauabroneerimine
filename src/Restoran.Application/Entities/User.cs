namespace Restoran.Application.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string PasswordHash { get; set; } = "";
    public UserRole Role { get; set; } = UserRole.Customer;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<Reservation> Reservations { get; set; } = new();
    public List<Notification> Notifications { get; set; } = new();
}
