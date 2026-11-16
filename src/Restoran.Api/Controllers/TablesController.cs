using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restoran.Application.DTOs;
using Restoran.Application.Interfaces;

namespace Restoran.Api.Controllers;

/// <summary>Laudade haldus.</summary>
[ApiController]
[Route("api/tables")]
[Produces("application/json")]
public class TablesController : ControllerBase
{
    private readonly ITableService _tables;

    public TablesController(ITableService tables)
    {
        _tables = tables;
    }

    /// <summary>Üks laud.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TableDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TableDto>> Get(int id) => Ok(await _tables.GetByIdAsync(id));

    /// <summary>Lisa laud.</summary>
    /// <remarks>
    /// Näide:
    ///
    ///     POST /api/tables
    ///     { "restaurantId": 1, "number": 11, "seats": 4, "location": "Hall", "x": 50, "y": 10, "width": 12, "height": 12 }
    /// </remarks>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(TableDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TableDto>> Create(SaveTableDto dto)
    {
        var created = await _tables.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /// <summary>Muuda lauda.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TableDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TableDto>> Update(int id, SaveTableDto dto) => Ok(await _tables.UpdateAsync(id, dto));

    /// <summary>Kustuta laud (kui sellel pole tulevasi broneeringuid).</summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        await _tables.DeleteAsync(id);
        return NoContent();
    }
}
