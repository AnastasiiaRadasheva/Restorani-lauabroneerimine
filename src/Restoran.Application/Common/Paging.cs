using System.ComponentModel.DataAnnotations;

namespace Restoran.Application.Common;

/// <summary>Lehekülgedeks jagatud vastus.</summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

/// <summary>Ühised lehekülgede ja sortimise parameetrid.</summary>
public class PageQuery
{
    /// <summary>Lehekülje number (alates 1).</summary>
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    /// <summary>Kirjeid leheküljel (1-100).</summary>
    [Range(1, 100)]
    public int PageSize { get; set; } = 10;

    /// <summary>Otsingusõna.</summary>
    public string? Search { get; set; }

    /// <summary>Sortimise väli.</summary>
    public string? Sort { get; set; }

    /// <summary>Kas sortida kahanevalt.</summary>
    public bool Desc { get; set; }
}
