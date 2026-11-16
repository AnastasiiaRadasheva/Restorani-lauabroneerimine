using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restoran.Application.Common;
using Restoran.Application.DTOs;
using Restoran.Application.Interfaces;

namespace Restoran.Api.Controllers;

/// <summary>Menüü.</summary>
[ApiController]
[Route("api/menu")]
[Produces("application/json")]
public class MenuController : ControllerBase
{
    private readonly IMenuService _menu;

    public MenuController(IMenuService menu)
    {
        _menu = menu;
    }

    /// <summary>Menüü koos otsingu, filtri, sortimise ja lehekülgedega.</summary>
    /// <remarks>Näide: GET /api/menu?restaurantId=1&amp;category=Pearoad&amp;sort=price&amp;desc=true&amp;page=1&amp;pageSize=10</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<MenuItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<MenuItemDto>>> GetAll([FromQuery] MenuQuery query) =>
        Ok(await _menu.GetPagedAsync(query));

    /// <summary>Üks menüüpunkt.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MenuItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MenuItemDto>> Get(int id) => Ok(await _menu.GetByIdAsync(id));

    /// <summary>Lisa menüüpunkt.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(MenuItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MenuItemDto>> Create(SaveMenuItemDto dto)
    {
        var created = await _menu.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /// <summary>Muuda menüüpunkti.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(MenuItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MenuItemDto>> Update(int id, SaveMenuItemDto dto) => Ok(await _menu.UpdateAsync(id, dto));

    /// <summary>Kustuta menüüpunkt.</summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        await _menu.DeleteAsync(id);
        return NoContent();
    }
}
