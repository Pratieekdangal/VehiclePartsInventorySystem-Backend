using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsSystem.Application.Common.Interfaces;
using VehiclePartsSystem.Application.DTOs;
using VehiclePartsSystem.Application.Services;

namespace VehiclePartsSystem.API.Controllers;

[ApiController]
[Route("api/appointments")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _service;
    private readonly ICurrentUserService _current;

    public AppointmentsController(IAppointmentService service, ICurrentUserService current)
    {
        _service = service;
        _current = current;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("me")]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetMine()
    {
        var id = _current.UserId ?? throw new UnauthorizedAccessException();
        return Ok(await _service.GetByCustomerAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> Create([FromBody] CreateAppointmentRequest request)
    {
        var id = _current.UserId ?? throw new UnauthorizedAccessException();
        return Ok(await _service.CreateAsync(id, request));
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<AppointmentDto>> SetStatus(Guid id, [FromBody] UpdateAppointmentStatusRequest request)
        => Ok(await _service.UpdateStatusAsync(id, request.Status));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) { await _service.DeleteAsync(id); return NoContent(); }
}
