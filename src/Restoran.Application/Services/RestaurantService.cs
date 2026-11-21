using Restoran.Application.Common;
using Restoran.Application.DTOs;
using Restoran.Application.Entities;
using Restoran.Application.Interfaces;
using Restoran.Application.Mapping;

namespace Restoran.Application.Services;

public class RestaurantService : IRestaurantService
{
    private readonly ICurrentUser _current;
    private readonly IRestaurantRepository _restaurants;

    public RestaurantService(IRestaurantRepository restaurants, ICurrentUser current)
    {
        _current = current;
        _restaurants = restaurants;
    }

    public async Task<List<RestaurantDto>> GetAllAsync(string? search, double? lat, double? lng)
    {
        var list = await _restaurants.GetAllAsync(search);
        var result = new List<RestaurantDto>();
        foreach (var r in list)
        {
            double? distance = lat.HasValue && lng.HasValue
                ? Geo.DistanceMeters(lat.Value, lng.Value, r.Latitude, r.Longitude)
                : null;
            result.Add(Localize(r.ToDto(r.Tables.Count(t => t.IsActive), distance)));
        }

        return lat.HasValue && lng.HasValue
            ? result.OrderBy(r => r.DistanceMeters).ToList()
            : result.OrderBy(r => r.Name).ToList();
    }

    public async Task<RestaurantDto> GetByIdAsync(int id)
    {
        var r = await Find(id);
        return Localize(r.ToDto(await _restaurants.CountTablesAsync(id)));
    }

    public async Task<RestaurantDto> CreateAsync(SaveRestaurantDto dto)
    {
        Validate(dto);
        var r = new Restaurant();
        r.Apply(dto);
        await _restaurants.AddAsync(r);
        await _restaurants.SaveChangesAsync();
        return r.ToDto(0);
    }

    public async Task<RestaurantDto> UpdateAsync(int id, SaveRestaurantDto dto)
    {
        Validate(dto);
        var r = await Find(id);
        r.Apply(dto);
        await _restaurants.SaveChangesAsync();
        return r.ToDto(await _restaurants.CountTablesAsync(id));
    }

    public async Task DeleteAsync(int id)
    {
        var r = await Find(id);
        _restaurants.Remove(r);
        await _restaurants.SaveChangesAsync();
    }

    private async Task<Restaurant> Find(int id) =>
        await _restaurants.GetByIdAsync(id) ?? throw new NotFoundException($"Restorani {id} ei leitud.");

    private static void Validate(SaveRestaurantDto dto)
    {
        if (!TimeOnly.TryParse(dto.OpensAt, out var opens) || !TimeOnly.TryParse(dto.ClosesAt, out var closes))
            throw new BusinessRuleException("Kellaaeg peab olema kujul HH:mm.");
        if (closes <= opens)
            throw new BusinessRuleException("Sulgemisaeg peab olema hilisem kui avamisaeg.");
    }

    /// <summary>Restorani kirjeldus saidi keeles.</summary>
    private RestaurantDto Localize(RestaurantDto d) => d with { Description = L.Word(d.Description, _current.Language) };
}
