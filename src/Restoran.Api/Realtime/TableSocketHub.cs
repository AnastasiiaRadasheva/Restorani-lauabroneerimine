using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Restoran.Application.DTOs;
using Restoran.Application.Interfaces;
using Restoran.Application.Services;

namespace Restoran.Api.Realtime;

/// <summary>
/// WebSocketi kanal /ws/tables.
/// Klient avab püsiühenduse (JWT query-parameetris access_token) ja server saadab sõnumi kohe,
/// kui laudade andmed muutuvad.
///
/// Sõnumid serverist:
///   ReservationCreated  – ainult personalile (Admin, Waiter), sisaldab kliendi nime
///   TableStatusChanged  – kõigile (broneerimise leht uuendab vabu laudu)
///   TableSelecting      – keegi valib praegu seda lauda (oranž laud plaanil)
///   SupportMessage      – uus sõnum klienditoe vestluses (sellele kliendile või külalisele ja personalile)
/// Külaline ilma kontota ühendub aadressil /ws/support?guest=võti ja saab ainult oma vestluse sõnumeid.
/// Sõnum kliendilt:
///   { "type": "Selecting", "restaurantId": 1, "tableId": 5, "startTime": "...", "selecting": true }
/// </summary>
public class TableSocketHub : IRealtimeNotifier
{
    private sealed class Client
    {
        public required WebSocket Socket { get; init; }
        public required int UserId { get; init; }
        public required bool IsStaff { get; init; }
        /// <summary>Külaline ilma kontota (ainult klienditoe vestlus).</summary>
        public string? GuestKey { get; init; }
        public SemaphoreSlim SendLock { get; } = new(1, 1);
    }

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly ConcurrentDictionary<Guid, Client> _clients = new();
    private readonly ILogger<TableSocketHub> _logger;

    public TableSocketHub(ILogger<TableSocketHub> logger)
    {
        _logger = logger;
    }

    public int ConnectionCount => _clients.Count;

    /// <summary>Teenindab ühte WebSocketi ühendust kuni see suletakse.</summary>
    public async Task HandleAsync(HttpContext context)
    {
        var user = context.User;
        var role = user.FindFirstValue(ClaimTypes.Role);
        await RunAsync(context, new Client
        {
            Socket = await context.WebSockets.AcceptWebSocketAsync(),
            UserId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!),
            IsStaff = role is "Admin" or "Waiter"
        }, role);
    }

    /// <summary>Külalise ühendus (/ws/support?guest=võti): saab ainult oma vestluse sõnumeid.</summary>
    public async Task HandleGuestAsync(HttpContext context, string guestKey) =>
        await RunAsync(context, new Client
        {
            Socket = await context.WebSockets.AcceptWebSocketAsync(),
            UserId = 0,
            IsStaff = false,
            GuestKey = guestKey
        }, "Guest");

    private async Task RunAsync(HttpContext context, Client client, string? role)
    {
        var id = Guid.NewGuid();
        _clients[id] = client;
        _logger.LogInformation("WebSocket ühendus avati: kasutaja {UserId} ({Role}), ühendusi {Count}", client.UserId, role, _clients.Count);

        var buffer = new byte[4096];
        try
        {
            while (client.Socket.State == WebSocketState.Open)
            {
                var result = await client.Socket.ReceiveAsync(buffer, context.RequestAborted);
                if (result.MessageType == WebSocketMessageType.Close) break;
                if (result.MessageType == WebSocketMessageType.Text && result.EndOfMessage && client.GuestKey is null)
                    await OnClientMessage(id, Encoding.UTF8.GetString(buffer, 0, result.Count));
            }
        }
        catch (Exception ex) when (ex is WebSocketException or OperationCanceledException)
        {
            // klient kadus (brauser suleti)
        }
        finally
        {
            _clients.TryRemove(id, out _);
            if (client.Socket.State == WebSocketState.Open)
                await client.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
            _logger.LogInformation("WebSocket ühendus suleti, ühendusi {Count}", _clients.Count);
        }
    }

    private record SelectingMessage(string Type, int RestaurantId, int TableId, DateTime StartTime, bool Selecting);

    /// <summary>Klient teatab, et valib lauda – edastame teistele (oranž laud).</summary>
    private async Task OnClientMessage(Guid senderId, string text)
    {
        SelectingMessage? msg;
        try { msg = JsonSerializer.Deserialize<SelectingMessage>(text, Json); }
        catch (JsonException) { return; }
        if (msg is null || msg.Type != "Selecting") return;

        await BroadcastAsync(new
        {
            type = "TableSelecting",
            msg.RestaurantId,
            msg.TableId,
            msg.StartTime,
            msg.Selecting
        }, c => c.GuestKey is null, except: senderId);
    }

    public Task ReservationCreatedAsync(ReservationDto r) =>
        BroadcastAsync(new
        {
            type = "ReservationCreated",
            r.RestaurantId,
            r.TableId,
            r.TableNumber,
            ReservationId = r.Id,
            r.StartTime,
            r.EndTime,
            r.Guests,
            r.CustomerName,
            r.Status
        }, c => c.IsStaff);

    public Task TableStatusChangedAsync(ReservationDto r) =>
        BroadcastAsync(new
        {
            type = "TableStatusChanged",
            r.RestaurantId,
            r.TableId,
            r.TableNumber,
            ReservationId = r.Id,
            r.StartTime,
            r.EndTime,
            r.Status
        }, c => c.GuestKey is null);

    public Task UserNotifiedAsync(int userId) =>
        BroadcastAsync(new { type = "Notification", userId }, c => c.UserId == userId);

    /// <summary>Klienditoe vestlus: sõnum jõuab kohe kliendile ja kogu personalile.</summary>
    public Task SupportMessageAsync(int? customerUserId, string? guestKey, SupportMessageDto message) =>
        BroadcastAsync(new { type = "SupportMessage", userId = customerUserId, guestKey, message },
            c => c.IsStaff || (customerUserId is not null && c.UserId == customerUserId) || (guestKey is not null && c.GuestKey == guestKey));

    private async Task BroadcastAsync(object message, Func<Client, bool> filter, Guid? except = null)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(message, Json);
        foreach (var (id, client) in _clients)
        {
            if (id == except || !filter(client) || client.Socket.State != WebSocketState.Open) continue;
            await client.SendLock.WaitAsync();
            try
            {
                await client.Socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (WebSocketException)
            {
                _clients.TryRemove(id, out _);
            }
            finally
            {
                client.SendLock.Release();
            }
        }
    }
}
