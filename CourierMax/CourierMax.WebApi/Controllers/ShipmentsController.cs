using Microsoft.AspNetCore.Mvc;
using CourierMax.Application.DTOs;
using CourierMax.Application.Services;

namespace CourierMax.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShipmentsController : ControllerBase
{
    private readonly IShipmentService _shipmentService;

    public ShipmentsController(IShipmentService shipmentService)
    {
        _shipmentService = shipmentService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateShipmentRequest request)
    {
        var shipment = await _shipmentService.CreateAsync(request);
        return CreatedAtAction(nameof(GetByTrackingCode), new { trackingCode = shipment.TrackingCode }, shipment);
    }

    [HttpGet("{trackingCode}")]
    [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByTrackingCode(string trackingCode)
    {
        var shipment = await _shipmentService.GetByTrackingCodeAsync(trackingCode);
        if (shipment is null)
            return NotFound(new { error = $"Shipment with tracking code '{trackingCode}' not found" });

        return Ok(shipment);
    }

    [HttpPost("{id}/assign")]
    [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Assign(int id, [FromBody] AssignRequest request)
    {
        var shipment = await _shipmentService.AssignAsync(id, request);
        return Ok(shipment);
    }

    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        var shipment = await _shipmentService.UpdateStatusAsync(id, request);
        return Ok(shipment);
    }

    [HttpGet("{id}/history")]
    [ProducesResponseType(typeof(IEnumerable<ShipmentHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistory(int id)
    {
        var history = await _shipmentService.GetHistoryAsync(id);
        return Ok(history);
    }
}
