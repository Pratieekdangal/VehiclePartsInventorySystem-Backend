using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsSystem.Application.Common.Interfaces;
using VehiclePartsSystem.Application.DTOs;
using VehiclePartsSystem.Application.Services;

namespace VehiclePartsSystem.API.Controllers;

[ApiController]
[Route("api/sales")]
[Authorize]
public class SalesController : ControllerBase
{
    private readonly ISalesService _service;
    private readonly ICurrentUserService _current;

    public SalesController(ISalesService service, ICurrentUserService current)
    {
        _service = service;
        _current = current;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<IEnumerable<SalesInvoiceDto>>> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("me")]
    public async Task<ActionResult<IEnumerable<SalesInvoiceDto>>> GetMine()
    {
        var id = _current.UserId ?? throw new UnauthorizedAccessException();
        return Ok(await _service.GetByCustomerAsync(id));
    }

    [HttpGet("customer/{customerId:guid}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<IEnumerable<SalesInvoiceDto>>> GetByCustomer(Guid customerId)
        => Ok(await _service.GetByCustomerAsync(customerId));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SalesInvoiceDto>> Get(Guid id) => Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<ActionResult<SalesInvoiceDto>> Create([FromBody] CreateSalesInvoiceRequest request)
    {
        var staffId = _current.UserId ?? throw new UnauthorizedAccessException();
        return Ok(await _service.CreateAsync(staffId, request));
    }

    [HttpPost("{id:guid}/email")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> Email(Guid id) { await _service.EmailInvoiceAsync(id); return NoContent(); }

    [HttpPost("{id:guid}/payment")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<ActionResult<SalesInvoiceDto>> RecordPayment(Guid id, [FromBody] PaymentRequest request)
        => Ok(await _service.RecordPaymentAsync(id, request.Amount));
}

public record PaymentRequest(decimal Amount);
