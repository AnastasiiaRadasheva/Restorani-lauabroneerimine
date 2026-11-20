using Restoran.Application.DTOs;

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
}
