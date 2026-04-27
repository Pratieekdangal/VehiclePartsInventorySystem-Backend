using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsSystem.Application.Common.Interfaces;
using VehiclePartsSystem.Application.DTOs;
using VehiclePartsSystem.Application.Services;

namespace VehiclePartsSystem.API.Controllers;

[ApiController]
[Route("api/purchases")]
[Authorize(Roles = "Admin")]
public class PurchasesController : ControllerBase
{
    private readonly IPurchaseService _service;
    private readonly ICurrentUserService _current;

    public PurchasesController(IPurchaseService service, ICurrentUserService current)
    {
        _service = service;
        _current = current;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PurchaseInvoiceDto>>> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PurchaseInvoiceDto>> Get(Guid id) => Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<ActionResult<PurchaseInvoiceDto>> Create([FromBody] CreatePurchaseInvoiceRequest request)
    {
        var adminId = _current.UserId ?? throw new UnauthorizedAccessException();
        return Ok(await _service.CreateAsync(adminId, request));
    }
}
