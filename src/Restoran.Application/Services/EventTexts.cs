using System.Text.Json;
using Restoran.Application.DTOs;

namespace Restoran.Application.Services;

/// <summary>
/// Õhtu tekstid teistes keeltes. Eesti keeles on põhitekst (Title, Kind, Description, Program);
/// admin võib lisada vene ja inglise tõlke – veebileht näitab neid valitud keeles.
/// </summary>
public static class EventTexts
{
    public static readonly string[] Languages = { "ru", "en" };

    /// <summary>Jätab alles ainult ru/en ja mittetühjad väljad, lõikab tühikud ära.</summary>
    public static Dictionary<string, EventTextDto> Normalize(Dictionary<string, EventTextDto>? input)
    {
        var result = new Dictionary<string, EventTextDto>();
        if (input is null) return result;
        foreach (var (lang, t) in input)
        {
            var key = lang.Trim().ToLowerInvariant();
            if (!Languages.Contains(key) || t is null) continue;
            var clean = new EventTextDto
            {
                Title = Clean(t.Title, 100), Kind = Clean(t.Kind, 100),
                Description = Clean(t.Description, 1000), Program = Clean(t.Program, 300)
            };
            if (clean.Title is not null || clean.Kind is not null || clean.Description is not null || clean.Program is not null)
                result[key] = clean;
        }
        return result;
    }

    public static string? Serialize(Dictionary<string, EventTextDto>? input)
    {
        var clean = Normalize(input);
        return clean.Count == 0 ? null : JsonSerializer.Serialize(clean);
    }

    public static Dictionary<string, EventTextDto> Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new();
        try { return Normalize(JsonSerializer.Deserialize<Dictionary<string, EventTextDto>>(json)); }
        catch (JsonException) { return new(); }
    }

    private static string? Clean(string? s, int max)
    {
        s = s?.Trim();
        if (string.IsNullOrEmpty(s)) return null;
        return s.Length > max ? s[..max] : s;
    }
}
