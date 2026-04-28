using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsSystem.Application.Common.Interfaces;
using VehiclePartsSystem.Application.DTOs;
using VehiclePartsSystem.Application.Services;

namespace VehiclePartsSystem.API.Controllers;

[ApiController]
[Route("api/part-requests")]
[Authorize]
public class PartRequestsController : ControllerBase
{
    private readonly IPartRequestService _service;
    private readonly ICurrentUserService _current;

    public PartRequestsController(IPartRequestService service, ICurrentUserService current)
    {
        _service = service;
        _current = current;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<IEnumerable<PartRequestDto>>> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("me")]
    public async Task<ActionResult<IEnumerable<PartRequestDto>>> GetMine()
    {
        var id = _current.UserId ?? throw new UnauthorizedAccessException();
        return Ok(await _service.GetByCustomerAsync(id));
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<PartRequestDto>> Create([FromBody] CreatePartRequestRequest request)
    {
        var id = _current.UserId ?? throw new UnauthorizedAccessException();
        return Ok(await _service.CreateAsync(id, request));
    }

    [HttpPut("{id:guid}/respond")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<PartRequestDto>> Respond(Guid id, [FromBody] RespondToPartRequestRequest request)
        => Ok(await _service.RespondAsync(id, request));
}
