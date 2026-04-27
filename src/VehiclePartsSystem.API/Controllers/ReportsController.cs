using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsSystem.Application.Common.Interfaces;
using VehiclePartsSystem.Application.DTOs;
using VehiclePartsSystem.Application.Services;

namespace VehiclePartsSystem.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;
    private readonly ICurrentUserService _current;

    public ReportsController(IReportService service, ICurrentUserService current)
    {
        _service = service;
        _current = current;
    }

    [HttpGet("admin/dashboard")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DashboardStatsDto>> AdminDashboard() => Ok(await _service.GetAdminStatsAsync());

    [HttpGet("staff/dashboard")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<ActionResult<StaffDashboardStatsDto>> StaffDashboard() => Ok(await _service.GetStaffStatsAsync());

    [HttpGet("customer/dashboard")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<CustomerDashboardStatsDto>> CustomerDashboard()
    {
        var id = _current.UserId ?? throw new UnauthorizedAccessException();
        return Ok(await _service.GetCustomerStatsAsync(id));
    }

    [HttpGet("financial")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<FinancialReportDto>> Financial([FromQuery] string range = "monthly")
        => Ok(await _service.GetFinancialReportAsync(range));

    [HttpGet("customers/top-spenders")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<IEnumerable<CustomerSpendingDto>>> TopSpenders([FromQuery] int top = 10)
        => Ok(await _service.GetTopSpendersAsync(top));

    [HttpGet("customers/regulars")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<IEnumerable<CustomerSpendingDto>>> Regulars([FromQuery] int minInvoices = 3)
        => Ok(await _service.GetRegularsAsync(minInvoices));

    [HttpGet("customers/pending-credits")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<IEnumerable<CustomerSpendingDto>>> PendingCredits()
        => Ok(await _service.GetPendingCreditsAsync());
}
