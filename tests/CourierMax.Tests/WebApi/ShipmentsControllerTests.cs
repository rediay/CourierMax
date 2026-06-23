using Moq;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using CourierMax.Application.DTOs;
using CourierMax.Application.Services;
using CourierMax.WebApi.Controllers;
using CourierMax.Domain.Entities;
using CourierMax.Domain.Enums;
using CourierMax.Domain.ValueObjects;
using CourierMax.Domain.Interfaces;

namespace CourierMax.Tests.WebApi.Controllers;

public class ShipmentsControllerTests
{
    private readonly Mock<IShipmentService> _mockService;
    private readonly ShipmentsController _controller;

    public ShipmentsControllerTests()
    {
        _mockService = new Mock<IShipmentService>();
        _controller = new ShipmentsController(_mockService.Object);
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreated()
    {
        var request = new CreateShipmentRequest
        {
            SenderName = "Juan Perez",
            SenderPhone = "3001234567",
            SenderAddress = "Calle 123",
            RecipientName = "Maria Lopez",
            RecipientPhone = "3102345678",
            RecipientAddress = "Carrera 456",
            PackageWeight = 5,
            PackageLength = 30,
            PackageWidth = 20,
            PackageHeight = 10,
            PackageType = PackageType.Paquete,
            ServiceType = ServiceType.Estandar,
            Origin = "Bogotá",
            Destination = "Medellín"
        };

        var expectedResponse = new ShipmentResponse
        {
            Id = 1,
            TrackingCode = "CM-12345678",
            SenderName = "Juan Perez",
            Status = "CREADO"
        };

        _mockService.Setup(s => s.CreateAsync(request))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.Create(request);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(ShipmentsController.GetByTrackingCode));
        createdResult.RouteValues!["trackingCode"].Should().Be("CM-12345678");
        createdResult.Value.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task GetByTrackingCode_ExistingCode_ReturnsOk()
    {
        var response = new ShipmentResponse
        {
            Id = 1,
            TrackingCode = "CM-12345678",
            Status = "CREADO"
        };

        _mockService.Setup(s => s.GetByTrackingCodeAsync("CM-12345678"))
            .ReturnsAsync(response);

        var result = await _controller.GetByTrackingCode("CM-12345678");

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(response);
    }

    [Fact]
    public async Task GetByTrackingCode_NonExistingCode_ReturnsNotFound()
    {
        _mockService.Setup(s => s.GetByTrackingCodeAsync("CM-99999999"))
            .ReturnsAsync((ShipmentResponse?)null);

        var result = await _controller.GetByTrackingCode("CM-99999999");

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Assign_ValidRequest_ReturnsOk()
    {
        var request = new AssignRequest { VehicleId = 1, DriverId = 2, ChangedBy = "operator" };
        var expectedResponse = new ShipmentResponse
        {
            Id = 1,
            TrackingCode = "CM-12345678",
            Status = "ASIGNADO",
            VehicleId = 1,
            DriverId = 2
        };

        _mockService.Setup(s => s.AssignAsync(1, request))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.Assign(1, request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task Assign_ServiceThrowsKeyNotFound_PropagatesException()
    {
        var request = new AssignRequest { VehicleId = 1, DriverId = 2, ChangedBy = "operator" };

        _mockService.Setup(s => s.AssignAsync(999, request))
            .ThrowsAsync(new KeyNotFoundException("Shipment with id 999 not found."));

        Func<Task> act = () => _controller.Assign(999, request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
