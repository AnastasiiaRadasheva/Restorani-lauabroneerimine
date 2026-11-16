using Restoran.Application.Common;
using Restoran.Application.DTOs;
using Restoran.Application.Entities;
using Restoran.Application.Interfaces;
using Restoran.Application.Mapping;

namespace Restoran.Application.Services;

public class TableService : ITableService
{
    private readonly ITableRepository _tables;
    private readonly IRestaurantRepository _restaurants;

    public TableService(ITableRepository tables, IRestaurantRepository restaurants)
    {
        _tables = tables;
        _restaurants = restaurants;
    }

    public async Task<List<TableDto>> GetByRestaurantAsync(int restaurantId)
    {
        _ = await _restaurants.GetByIdAsync(restaurantId)
            ?? throw new NotFoundException($"Restorani {restaurantId} ei leitud.");
        var tables = await _tables.GetByRestaurantAsync(restaurantId, onlyActive: false);
        return tables.Select(t => t.ToDto()).ToList();
    }

    public async Task<TableDto> GetByIdAsync(int id) => (await Find(id)).ToDto();

    public async Task<TableDto> CreateAsync(SaveTableDto dto)
    {
        await ValidateAsync(dto, null);
        var t = new DiningTable();
        t.Apply(dto);
        await _tables.AddAsync(t);
        await _tables.SaveChangesAsync();
        return t.ToDto();
    }

    public async Task<TableDto> UpdateAsync(int id, SaveTableDto dto)
    {
        var t = await Find(id);
        await ValidateAsync(dto, id);
        t.Apply(dto);
        await _tables.SaveChangesAsync();
        return t.ToDto();
    }

    public async Task DeleteAsync(int id)
    {
        var t = await Find(id);
        if (await _tables.HasFutureReservationsAsync(id))
            throw new ConflictException("Laual on tulevasi broneeringuid. Märgi laud mitteaktiivseks.");
        _tables.Remove(t);
        await _tables.SaveChangesAsync();
    }

    private async Task<DiningTable> Find(int id) =>
        await _tables.GetByIdAsync(id) ?? throw new NotFoundException($"Lauda {id} ei leitud.");

    private async Task ValidateAsync(SaveTableDto dto, int? id)
    {
        _ = await _restaurants.GetByIdAsync(dto.RestaurantId)
            ?? throw new NotFoundException($"Restorani {dto.RestaurantId} ei leitud.");
        if (await _tables.NumberExistsAsync(dto.RestaurantId, dto.Number, id))
            throw new ConflictException($"Laud numbriga {dto.Number} on selles restoranis juba olemas.");
    }
}
