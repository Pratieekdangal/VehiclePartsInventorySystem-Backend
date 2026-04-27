using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsSystem.Application.Common.Interfaces;
using VehiclePartsSystem.Application.DTOs;
using VehiclePartsSystem.Application.Services;

namespace VehiclePartsSystem.API.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _service;
    private readonly ICurrentUserService _current;

    public CustomersController(ICustomerService service, ICurrentUserService current)
    {
        _service = service;
        _current = current;
    }

    [HttpGet("search")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<IEnumerable<CustomerSummaryDto>>> Search(
        [FromQuery] string? query, [FromQuery] string? vehicleNumber)
        => Ok(await _service.SearchAsync(query, vehicleNumber));

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<CustomerDetailDto>> Get(Guid id) => Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<CustomerSummaryDto>> Create([FromBody] CreateCustomerByStaffRequest request)
        => Ok(await _service.CreateByStaffAsync(request));

    [HttpGet("me")]
    public async Task<ActionResult<CustomerDetailDto>> GetMe()
    {
        var id = _current.UserId ?? throw new UnauthorizedAccessException();
        return Ok(await _service.GetByIdAsync(id));
    }

    [HttpPut("me")]
    public async Task<ActionResult<CustomerDetailDto>> UpdateMe([FromBody] UpdateProfileRequest request)
    {
        var id = _current.UserId ?? throw new UnauthorizedAccessException();
        return Ok(await _service.UpdateProfileAsync(id, request));
    }

    [HttpGet("me/vehicles")]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> MyVehicles()
    {
        var id = _current.UserId ?? throw new UnauthorizedAccessException();
        return Ok(await _service.GetVehiclesAsync(id));
    }

    [HttpPost("me/vehicles")]
    public async Task<ActionResult<VehicleDto>> AddMyVehicle([FromBody] CreateVehicleRequest request)
    {
        var id = _current.UserId ?? throw new UnauthorizedAccessException();
        return Ok(await _service.AddVehicleAsync(id, request));
    }

    [HttpPut("me/vehicles/{vehicleId:guid}")]
    public async Task<ActionResult<VehicleDto>> UpdateMyVehicle(Guid vehicleId, [FromBody] UpdateVehicleRequest request)
    {
        var id = _current.UserId ?? throw new UnauthorizedAccessException();
        return Ok(await _service.UpdateVehicleAsync(id, vehicleId, request));
    }

    [HttpDelete("me/vehicles/{vehicleId:guid}")]
    public async Task<IActionResult> DeleteMyVehicle(Guid vehicleId)
    {
        var id = _current.UserId ?? throw new UnauthorizedAccessException();
        await _service.DeleteVehicleAsync(id, vehicleId);
        return NoContent();
    }

    [HttpGet("{id:guid}/vehicles")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> Vehicles(Guid id)
        => Ok(await _service.GetVehiclesAsync(id));

    [HttpPost("{id:guid}/vehicles")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<VehicleDto>> AddVehicle(Guid id, [FromBody] CreateVehicleRequest request)
        => Ok(await _service.AddVehicleAsync(id, request));
}
