using Microsoft.AspNetCore.Mvc;
using Restoran.Application.DTOs;
using Restoran.Application.Interfaces;

namespace Restoran.Api.Controllers;

/// <summary>Restoranid.</summary>
[ApiController]
[Route("api/restaurants")]
[Produces("application/json")]
public class RestaurantsController : ControllerBase
{
    private readonly IRestaurantService _restaurants;
    private readonly ITableService _tables;

    public RestaurantsController(IRestaurantService restaurants, ITableService tables)
    {
        _restaurants = restaurants;
        _tables = tables;
    }

    /// <summary>Kõik restoranid. Kui lat ja lng on antud, sorditakse lähima järgi ja lisatakse kaugus meetrites.</summary>
    /// <param name="search">Otsing nime või aadressi järgi.</param>
    /// <param name="lat">Kasutaja laiuskraad, nt 59.437.</param>
    /// <param name="lng">Kasutaja pikkuskraad, nt 24.745.</param>
    [HttpGet]
    [ProducesResponseType(typeof(List<RestaurantDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RestaurantDto>>> GetAll(string? search, double? lat, double? lng) =>
        Ok(await _restaurants.GetAllAsync(search, lat, lng));

    /// <summary>Üks restoran.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RestaurantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RestaurantDto>> Get(int id) => Ok(await _restaurants.GetByIdAsync(id));

    /// <summary>Restorani kõik lauad.</summary>
    [HttpGet("{id:int}/tables")]
    [ProducesResponseType(typeof(List<TableDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<TableDto>>> GetTables(int id) => Ok(await _tables.GetByRestaurantAsync(id));

    /// <summary>Lisa restoran.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(RestaurantDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RestaurantDto>> Create(SaveRestaurantDto dto)
    {
        var created = await _restaurants.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /// <summary>Muuda restorani.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(RestaurantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RestaurantDto>> Update(int id, SaveRestaurantDto dto) =>
        Ok(await _restaurants.UpdateAsync(id, dto));

    /// <summary>Kustuta restoran.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _restaurants.DeleteAsync(id);
        return NoContent();
    }
}
