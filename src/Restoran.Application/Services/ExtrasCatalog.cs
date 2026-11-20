namespace Restoran.Application.Services;

/// <summary>Lisateenuse kirje kataloogis.</summary>
public record ExtraInfo(string Code, string Name, string Description, decimal Price);

/// <summary>Lisateenuste kataloog (sama kõigis restoranides).</summary>
public static class ExtrasCatalog
{
    public static readonly IReadOnlyList<ExtraInfo> All = new List<ExtraInfo>
    {
        new("flowers", "Lillekimp lauale", "hooajaline seade ootab Teid laual", 45m),
        new("bottle", "Pudel šampanjat", "brut, serveeritakse jahutatult Teie märguandel", 95m),
        new("bigcake", "Tellimustort, 1 kg", "kiri ja kaunistus erisoovides", 60m),
        new("violin", "Viiuldaja, 20 minutit", "elav muusika Teie laua juures", 120m),
        new("photo", "Fotograaf, 1 tund", "fotod saadetakse e-postile kolme päeva jooksul", 80m),
        new("chair", "Lastetool", "valmistatakse ette Teie saabumiseks", 0m),
    };

    public static ExtraInfo? Find(string code) => All.FirstOrDefault(e => e.Code == code);
}
