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
    DateTime StartTime,
    // Kliendi keel – meeldetuletus saadetakse selles keeles
    string CustomerLanguage = "et");

/// <summary>No-show töötluse tulemus.</summary>
public record NoShowResultDto(int Count, List<int> ReservationIds, List<UnpaidCancelledDto>? UnpaidCancelled = null);

/// <summary>Maksmata ja seetõttu tühistatud broneering või õhtu registreerimine – kliendile saadetakse teade.</summary>
public record UnpaidCancelledDto(int UserId, int? ReservationId, string What);

public record NotificationDto(int Id, int UserId, int? ReservationId, string Message, bool IsRead, DateTime CreatedAt, string? Link = null);

/// <summary>Uus teade kasutajale (saadab taustateenus).</summary>
public class CreateNotificationDto
{
    /// <example>3</example>
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    /// <example>2301</example>
    public int? ReservationId { get; set; }

    /// <summary>Kuhu teate klõps viib (valikuline).</summary>
    [StringLength(200)]
    public string? Link { get; set; }

    /// <example>Meeldetuletus: homme kell 19:00 ootab sind Aed Vanalinn.</example>
    [Required, StringLength(500, MinimumLength = 1)]
    public string Message { get; set; } = "";
}
