using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsSystem.Application.DTOs;
using VehiclePartsSystem.Application.Services;

namespace VehiclePartsSystem.API.Controllers;

[ApiController]
[Route("api/parts")]
[Authorize]
public class PartsController : ControllerBase
{
    private readonly IPartService _service;
    public PartsController(IPartService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PartDto>>> GetAll(
        [FromQuery] string? search, [FromQuery] string? category, [FromQuery] bool? lowStockOnly)
        => Ok(await _service.GetAllAsync(search, category, lowStockOnly));

    [HttpGet("low-stock")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<IEnumerable<PartDto>>> GetLowStock() => Ok(await _service.GetLowStockAsync());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PartDto>> Get(Guid id) => Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PartDto>> Create([FromBody] CreatePartRequest request)
        => Ok(await _service.CreateAsync(request));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PartDto>> Update(Guid id, [FromBody] UpdatePartRequest request)
        => Ok(await _service.UpdateAsync(id, request));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id) { await _service.DeleteAsync(id); return NoContent(); }
}
