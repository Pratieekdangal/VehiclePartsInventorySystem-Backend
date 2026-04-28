using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsSystem.Application.DTOs;
using VehiclePartsSystem.Application.Services;

namespace VehiclePartsSystem.API.Controllers;

[ApiController]
[Route("api/staff")]
[Authorize(Roles = "Admin")]
public class StaffController : ControllerBase
{
    private readonly IStaffService _service;
    public StaffController(IStaffService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StaffDto>>> GetAll() => Ok(await _service.GetAllAsync());

    [HttpPost]
    public async Task<ActionResult<StaffDto>> Create([FromBody] CreateStaffRequest request)
        => Ok(await _service.CreateAsync(request));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<StaffDto>> Update(Guid id, [FromBody] UpdateStaffRequest request)
        => Ok(await _service.UpdateAsync(id, request));

    [HttpPost("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordRequest request)
    {
        await _service.ResetPasswordAsync(id, request.NewPassword);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) { await _service.DeleteAsync(id); return NoContent(); }
}

public record ResetPasswordRequest(string NewPassword);
