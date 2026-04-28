using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsSystem.Application.DTOs;
using VehiclePartsSystem.Application.Services;

namespace VehiclePartsSystem.API.Controllers;

[ApiController]
[Route("api/vendors")]
[Authorize(Roles = "Admin")]
public class VendorsController : ControllerBase
{
    private readonly IVendorService _service;
    public VendorsController(IVendorService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VendorDto>>> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VendorDto>> Get(Guid id) => Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<ActionResult<VendorDto>> Create([FromBody] CreateVendorRequest request)
        => Ok(await _service.CreateAsync(request));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<VendorDto>> Update(Guid id, [FromBody] UpdateVendorRequest request)
        => Ok(await _service.UpdateAsync(id, request));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) { await _service.DeleteAsync(id); return NoContent(); }
}
