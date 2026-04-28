using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsSystem.Application.Common.Interfaces;
using VehiclePartsSystem.Application.DTOs;
using VehiclePartsSystem.Application.Services;

namespace VehiclePartsSystem.API.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _service;
    private readonly ICurrentUserService _current;

    public ReviewsController(IReviewService service, ICurrentUserService current)
    {
        _service = service;
        _current = current;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetVisible() => Ok(await _service.GetVisibleAsync());

    [HttpGet("all")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetAll() => Ok(await _service.GetAllAsync());

    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<ReviewDto>> Create([FromBody] CreateReviewRequest request)
    {
        var id = _current.UserId ?? throw new UnauthorizedAccessException();
        return Ok(await _service.CreateAsync(id, request));
    }

    [HttpPut("{id:guid}/visibility")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<ReviewDto>> SetVisibility(Guid id, [FromBody] VisibilityRequest request)
        => Ok(await _service.SetVisibilityAsync(id, request.Visible));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id) { await _service.DeleteAsync(id); return NoContent(); }
}

public record VisibilityRequest(bool Visible);
