using System.ComponentModel.DataAnnotations;

namespace Restoran.Application.DTOs;

/// <summary>Homne broneering, millele pole veel meeldetuletust saadetud.</summary>
public record RemindDueDto(
    int Id,
    int UserId,
    string CustomerName,
    string RestaurantName,
    int TableNumber,
    int Guests,
    DateTime StartTime);

/// <summary>No-show töötluse tulemus.</summary>
public record NoShowResultDto(int Count, List<int> ReservationIds);

public record NotificationDto(int Id, int UserId, int? ReservationId, string Message, bool IsRead, DateTime CreatedAt);

/// <summary>Uus teade kasutajale (saadab taustateenus).</summary>
public class CreateNotificationDto
{
    /// <example>3</example>
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    /// <example>2301</example>
    public int? ReservationId { get; set; }

    /// <example>Meeldetuletus: homme kell 19:00 ootab sind Aed Vanalinn.</example>
    [Required, StringLength(500, MinimumLength = 1)]
    public string Message { get; set; } = "";
}
