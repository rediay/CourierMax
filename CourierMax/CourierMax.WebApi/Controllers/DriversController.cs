using Microsoft.AspNetCore.Mvc;
using CourierMax.Application.DTOs;
using CourierMax.Application.Services;

namespace CourierMax.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DriversController : ControllerBase
{
    private readonly IDriverMetricsService _driverMetricsService;

    public DriversController(IDriverMetricsService driverMetricsService)
    {
        _driverMetricsService = driverMetricsService;
    }

    [HttpGet("{id}/efficiency-report")]
    [ProducesResponseType(typeof(DriverEfficiencyReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEfficiencyReport(int id)
    {
        var report = await _driverMetricsService.GetEfficiencyReportAsync(id);
        return Ok(report);
    }
}
