using Restoran.Application.DTOs;
using Restoran.Application.Services;

namespace Restoran.Application.Interfaces;

/// <summary>
/// Reaalajas teated (WebSocket /ws/tables).
/// Teenuste kiht kutsub neid välja PÄRAST andmete salvestamist.
/// </summary>
public interface IRealtimeNotifier
{
    /// <summary>Uus broneering. Täielik info (kliendi nimi) läheb ainult personalile.</summary>
    Task ReservationCreatedAsync(ReservationDto reservation);

    /// <summary>Laua olek muutus (uus broneering, tühistamine, saabus, no-show). Läheb kõigile.</summary>
    Task TableStatusChangedAsync(ReservationDto reservation);

    /// <summary>Kasutajale tuli uus teade (kelluke uueneb kohe).</summary>
    Task UserNotifiedAsync(int userId);

    /// <summary>Uus sõnum klienditoe vestluses. Läheb sellele kliendile (või külalisele võtme järgi) ja personalile.</summary>
    Task SupportMessageAsync(int? customerUserId, string? guestKey, SupportMessageDto message);
}
