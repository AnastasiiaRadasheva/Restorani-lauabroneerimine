namespace Restoran.Application.Services;

/// <summary>Õhtu erijook (ainult selle õhtu jaoks).</summary>
public record EventDrink(string Code, string Name, string Description, decimal Price);

/// <summary>Iga gastronoomilise õhtu oma joogid ja lubatud lisateenused.</summary>
public static class EventCatalog
{
    private static readonly Dictionary<string, List<EventDrink>> Drinks = new()
    {
        ["steel"] = new()
        {
            new("whisky", "Viskisaade, 3 klaasi", "suitsune Islay, Speyside ja Jaapani single malt", 45m),
            new("oldfashioned", "Suitsune Old Fashioned", "burbon, apelsin, tammesuits", 18m),
            new("barolo", "Barolo, klaas", "Piemonte, Nebbiolo", 22m),
        },
        ["white"] = new()
        {
            new("chablis", "Chablis Grand Cru, klaas", "Burgundia, mineraalne", 24m),
            new("blanc", "Šampanja Blanc de Blancs, klaas", "100% Chardonnay", 22m),
            new("burgundy", "Valge Burgundia saade, 5 klaasi", "iga käigu juurde oma vein", 70m),
        },
        ["black"] = new()
        {
            new("manhattan", "Black Manhattan", "rukkiviski, amaro, kirss", 19m),
            new("espresso", "Espresso Martini", "viin, espresso, kohvilikköör", 16m),
            new("rose", "Roosa šampanja, klaas", "brut rosé, Prantsusmaa", 24m),
        },
        ["truffle"] = new()
        {
            new("barbaresco", "Barbaresco, klaas", "Piemonte, Nebbiolo", 24m),
            new("trufflemartini", "Trühvlimartini", "džinn, kuiv vermut, trühvliõli", 17m),
        },
        ["oyster"] = new()
        {
            new("muscadet", "Muscadet, klaas", "Loire, mineraalne", 14m),
            new("brutnature", "Šampanja Brut Nature, klaas", "kuiv ja värske", 20m),
        },
        ["vinyl"] = new()
        {
            new("natural", "Naturaalvein, klaas", "väikeveinimajast, sommeljee valik", 12m),
            new("pinot", "Pinot Noir, klaas", "Burgundia", 18m),
        },
        ["spring"] = new()
        {
            new("rhubarb", "Rabarberi spritz", "rabarber, vahuvein, piparmünt", 12m),
            new("riesling", "Riesling, klaas", "Mosel, kerge", 13m),
        },
        ["midnight"] = new()
        {
            new("manhattan", "Black Manhattan", "rukkiviski, amaro, kirss", 19m),
            new("rose", "Roosa šampanja, klaas", "brut rosé, Prantsusmaa", 24m),
        },
    };

    /// <summary>Õhtutel lubatud lisateenused (viiuldajat jms ei saa – õhtul on oma muusika).</summary>
    public static readonly string[] AllowedExtras = { "bottle", "flowers" };

    public static List<EventDrink> DrinksFor(string code) => Drinks.TryGetValue(code, out var list) ? list : new();
}
