using Microsoft.Extensions.Options;

namespace Restoran.Worker;

/// <summary>
/// Taustateenus, mis käivitub iga 15 minuti järel:
///   1. GET  /api/reservations/remind-due        – homsed meeldetuletamata broneeringud
///   2. POST /api/notifications + PATCH /api/reservations/{id}/reminded
///   3. POST /api/reservations/mark-no-show      – vabastab lauad, kuhu klient 30 min jooksul ei tulnud
/// Äriloogika on API teenuste kihis, Worker ainult käivitab selle.
/// </summary>
public class ReminderWorker : BackgroundService
{
    private readonly ApiClient _api;
    private readonly ILogger<ReminderWorker> _logger;
    private readonly TimeSpan _interval;

    public ReminderWorker(ApiClient api, IOptions<ApiOptions> options, ILogger<ReminderWorker> logger)
    {
        _api = api;
        _logger = logger;
        _interval = TimeSpan.FromMinutes(Math.Max(1, options.Value.IntervalMinutes));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker käivitus, intervall {Interval} min", _interval.TotalMinutes);

        // Esimene kord kohe, siis iga intervalli järel
        await RunOnceAsync(stoppingToken);

        using var timer = new PeriodicTimer(_interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunOnceAsync(stoppingToken);
        }
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        try
        {
            await SendRemindersAsync(ct);
            await MarkNoShowsAsync(ct);
        }
        catch (HttpRequestException ex)
        {
            // API ei tööta – proovime järgmisel korral uuesti, Worker ei kuku kokku
            _logger.LogWarning("API pole kättesaadav: {Message}", ex.Message);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Tsükkel ebaõnnestus");
        }
    }

    private async Task SendRemindersAsync(CancellationToken ct)
    {
        var due = await _api.GetRemindDueAsync(ct);
        foreach (var r in due)
        {
            var message = $"Meeldetuletus: homme kell {r.StartTime:HH:mm} ootab sind {r.RestaurantName}, " +
                          $"laud {r.TableNumber} ({r.Guests} inimest). Muuta enam ei saa – küsimuste korral helista restorani.";
            await _api.SendNotificationAsync(r.UserId, r.Id, message, ct);
            await _api.MarkRemindedAsync(r.Id, ct);
        }
        _logger.LogInformation("Meeldetuletusi saadetud: {Count}", due.Count);
    }

    private async Task MarkNoShowsAsync(CancellationToken ct)
    {
        var result = await _api.MarkNoShowsAsync(ct);
        if (result.Count > 0)
            _logger.LogInformation("Vabastatud laudu (klient ei tulnud): {Count} – broneeringud {Ids}",
                result.Count, string.Join(", ", result.ReservationIds));
        else
            _logger.LogInformation("No-show broneeringuid pole");
    }
}
