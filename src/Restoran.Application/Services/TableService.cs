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
        await FillMissingAsync(dto);
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
        // Puuduvad väärtused jäävad samaks
        dto.Number ??= t.Number;
        dto.X ??= t.X; dto.Y ??= t.Y;
        dto.Width ??= t.Width; dto.Height ??= t.Height;
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

    /// <summary>
    /// Uue laua puuduvad andmed: järgmine vaba number, suurus kohtade arvu järgi (nagu admini lehel)
    /// ja esimene vaba koht plaanil, mis ei kattu teiste laudade, baari, lava ega sissepääsuga.
    /// Nii ei teki näiteks Swaggerist lisatud lauad üksteise peale.
    /// </summary>
    private async Task FillMissingAsync(SaveTableDto dto)
    {
        var restaurant = await _restaurants.GetByIdAsync(dto.RestaurantId)
            ?? throw new NotFoundException($"Restorani {dto.RestaurantId} ei leitud.");
        var all = await _tables.GetByRestaurantAsync(dto.RestaurantId, onlyActive: false) ?? new List<DiningTable>();
        dto.Number ??= all.Count == 0 ? 1 : all.Max(t => t.Number) + 1;
        if (dto.Width is null || dto.Height is null)
        {
            var (w, h) = SizeFor(dto.Seats, dto.IsRound);
            dto.Width ??= w;
            dto.Height ??= h;
        }
        if (dto.X is null || dto.Y is null)
        {
            var spot = FreeSpot(all.Where(t => t.Location == dto.Location), restaurant, dto.Location, dto.Width.Value, dto.Height.Value)
                ?? throw new ConflictException("Plaanil pole sellele lauale vaba kohta. Valige väiksem laud või teine saal.");
            dto.X = spot.X;
            dto.Y = spot.Y;
        }
    }

    private static (int W, int H) SizeFor(int seats, bool round)
    {
        if (round) return seats <= 2 ? (10, 16) : (13, 20);
        if (seats <= 2) return (10, 16);
        if (seats <= 4) return (14, 16);
        if (seats <= 6) return (14, 26);
        if (seats <= 8) return (22, 24);
        return (30, 26);
    }

    private static (int X, int Y)? FreeSpot(IEnumerable<DiningTable> tables, Restaurant r, TableLocation zone, int w, int h)
    {
        // Teised lauad koos 2 % vahega, baar, lava ja sissepääs
        var blocks = tables.Select(t => (X: t.X - 2, Y: t.Y - 2, W: t.Width + 4, H: t.Height + 4)).ToList();
        if (zone == TableLocation.Hall && r.BarWidth > 0) blocks.Add((r.BarX, r.BarY, r.BarWidth, r.BarHeight));
        if (r.StageZone == zone && r.StageWidth > 0) blocks.Add((r.StageX, r.StageY, r.StageWidth, r.StageHeight));
        blocks.Add((22, 86, 16, 14));
        for (var y = 4; y + h <= 98; y += 2)
            for (var x = 4; x + w <= 98; x += 2)
                if (!blocks.Any(b => x < b.X + b.W && x + w > b.X && y < b.Y + b.H && y + h > b.Y))
                    return (x, y);
        return null;
    }

    private async Task ValidateAsync(SaveTableDto dto, int? id)
    {
        int x = dto.X ?? 0, y = dto.Y ?? 0, width = dto.Width ?? 12, height = dto.Height ?? 12, number = dto.Number ?? 0;
        var restaurant = await _restaurants.GetByIdAsync(dto.RestaurantId)
            ?? throw new NotFoundException($"Restorani {dto.RestaurantId} ei leitud.");
        if (await _tables.NumberExistsAsync(dto.RestaurantId, number, id))
            throw new ConflictException($"Laud numbriga {number} on selles restoranis juba olemas.");

        // Laud peab jääma plaani sisse ega tohi kattuda teise lauaga samas saalis
        if (x + width > 100 || y + height > 100)
            throw new BusinessRuleException("Laud ei mahu plaanile – lohistage see saali sisse.");
        var others = (await _tables.GetByRestaurantAsync(dto.RestaurantId, onlyActive: false) ?? new List<DiningTable>())
            .Where(t => t.Id != id && t.Location == dto.Location);
        var hit = others.FirstOrDefault(t => x < t.X + t.Width && x + width > t.X && y < t.Y + t.Height && y + height > t.Y);
        if (hit is not null)
            throw new ConflictException($"Laud kattub lauaga {hit.Number}. Valige teine koht.");
        bool Covers(int bx, int by, int bw, int bh) => bw > 0 && x < bx + bw && x + width > bx && y < by + bh && y + height > by;
        if (dto.Location == TableLocation.Hall && Covers(restaurant.BarX, restaurant.BarY, restaurant.BarWidth, restaurant.BarHeight))
            throw new ConflictException("Laud ei saa olla baari peal. Valige teine koht.");
        if (dto.Location == restaurant.StageZone && Covers(restaurant.StageX, restaurant.StageY, restaurant.StageWidth, restaurant.StageHeight))
            throw new ConflictException("Laud ei saa olla laval. Valige teine koht.");
    }
}
