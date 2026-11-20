using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace Restoran.Worker;

public class ApiOptions
{
    public string BaseUrl { get; set; } = "http://localhost:5000";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public int IntervalMinutes { get; set; } = 15;
}

// Workeri enda DTO-d. Worker EI viita API, Application ega Infrastructure projektile
// ega DbContextile – ta räägib API-ga ainult HTTP kaudu.
public record LoginResponse(string Token, DateTime ExpiresAt);
public record RemindDue(int Id, int UserId, string CustomerName, string RestaurantName, int TableNumber, int Guests, DateTime StartTime);
public record NoShowResult(int Count, List<int> ReservationIds);

/// <summary>HttpClient, mis logib sisse Service rolliga kontoga ja lisab JWT tokeni igale päringule.</summary>
public class ApiClient
{
    private readonly HttpClient _http;
    private readonly ApiOptions _options;
    private readonly ILogger<ApiClient> _logger;
    private string? _token;
    private DateTime _tokenExpires;

    public ApiClient(HttpClient http, IOptions<ApiOptions> options, ILogger<ApiClient> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
        _http.BaseAddress = new Uri(_options.BaseUrl);
    }

    private async Task EnsureLoggedInAsync(CancellationToken ct)
    {
        if (_token is not null && _tokenExpires > DateTime.UtcNow.AddMinutes(5)) return;

        var response = await _http.PostAsJsonAsync("/api/auth/login",
            new { email = _options.Email, password = _options.Password }, ct);
        response.EnsureSuccessStatusCode();
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>(ct)
                    ?? throw new InvalidOperationException("Tühi login vastus");

        _token = login.Token;
        _tokenExpires = login.ExpiresAt;
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        _logger.LogInformation("Sisse logitud teenusekontoga {Email}", _options.Email);
    }

    /// <summary>Saadab päringu; kui token aegus (401), logib uuesti sisse ja proovib korra veel.</summary>
    private async Task<HttpResponseMessage> SendAsync(Func<Task<HttpResponseMessage>> send, CancellationToken ct)
    {
        await EnsureLoggedInAsync(ct);
        var response = await send();
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _token = null;
            await EnsureLoggedInAsync(ct);
            response = await send();
        }
        response.EnsureSuccessStatusCode();
        return response;
    }

    public async Task<List<RemindDue>> GetRemindDueAsync(CancellationToken ct)
    {
        var response = await SendAsync(() => _http.GetAsync("/api/reservations/remind-due", ct), ct);
        return await response.Content.ReadFromJsonAsync<List<RemindDue>>(ct) ?? new();
    }

    public Task SendNotificationAsync(int userId, int reservationId, string message, CancellationToken ct) =>
        SendAsync(() => _http.PostAsJsonAsync("/api/notifications", new { userId, reservationId, message }, ct), ct);

    public Task MarkRemindedAsync(int reservationId, CancellationToken ct) =>
        SendAsync(() => _http.PatchAsync($"/api/reservations/{reservationId}/reminded", null, ct), ct);

    public async Task<NoShowResult> MarkNoShowsAsync(CancellationToken ct)
    {
        var response = await SendAsync(() => _http.PostAsync("/api/reservations/mark-no-show", null, ct), ct);
        return await response.Content.ReadFromJsonAsync<NoShowResult>(ct) ?? new(0, new());
    }
}
