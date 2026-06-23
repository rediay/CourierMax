using Moq;
using FluentAssertions;
using CourierMax.Application.DTOs;
using CourierMax.Application.Services;
using CourierMax.Domain.Entities;
using CourierMax.Domain.Enums;
using CourierMax.Domain.Interfaces;
using CourierMax.Domain.ValueObjects;

namespace CourierMax.Tests.Application.Services;

public class ShipmentServiceTests
{
    private readonly Mock<IShipmentRepository> _mockRepo;
    private readonly ShipmentService _service;

    public ShipmentServiceTests()
    {
        _mockRepo = new Mock<IShipmentRepository>();
        _service = new ShipmentService(_mockRepo.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesAndReturnsShipment()
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

        Shipment? savedShipment = null;
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Shipment>()))
            .Callback<Shipment>(s => savedShipment = s)
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(request);

        result.Should().NotBeNull();
        result.TrackingCode.Should().MatchRegex(@"^CM-\d{8}$");
        result.SenderName.Should().Be("Juan Perez");
        result.Status.Should().Be("CREADO");

        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Shipment>()), Times.Once);
    }

    [Fact]
    public async Task GetByTrackingCodeAsync_ExistingCode_ReturnsShipment()
    {
        var shipment = new Shipment(
            "Juan Perez", new Phone("3001234567"), new Address("Calle 123"),
            "Maria Lopez", new Phone("3102345678"), new Address("Carrera 456"),
            new Weight(5), new Dimensions(30, 20, 10),
            PackageType.Paquete, ServiceType.Estandar, "Bogotá", "Medellín");

        _mockRepo.Setup(r => r.GetByTrackingCodeAsync(shipment.TrackingCode.ToString()))
            .ReturnsAsync(shipment);

        var result = await _service.GetByTrackingCodeAsync(shipment.TrackingCode.ToString());

        result.Should().NotBeNull();
        result!.TrackingCode.Should().Be(shipment.TrackingCode.ToString());
    }

    [Fact]
    public async Task GetByTrackingCodeAsync_NonExistingCode_ReturnsNull()
    {
        _mockRepo.Setup(r => r.GetByTrackingCodeAsync("CM-99999999"))
            .ReturnsAsync((Shipment?)null);

        var result = await _service.GetByTrackingCodeAsync("CM-99999999");

        result.Should().BeNull();
    }

    [Fact]
    public async Task AssignAsync_ExistingShipment_AssignsAndReturnsResponse()
    {
        var shipment = new Shipment(
            "Juan Perez", new Phone("3001234567"), new Address("Calle 123"),
            "Maria Lopez", new Phone("3102345678"), new Address("Carrera 456"),
            new Weight(5), new Dimensions(30, 20, 10),
            PackageType.Paquete, ServiceType.Estandar, "Bogotá", "Medellín");
        var request = new AssignRequest { VehicleId = 1, DriverId = 2, ChangedBy = "operator" };

        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(shipment);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Shipment>())).Returns(Task.CompletedTask);

        var result = await _service.AssignAsync(1, request);

        result.Should().NotBeNull();
        result.Status.Should().Be("ASIGNADO");
        result.VehicleId.Should().Be(1);
        result.DriverId.Should().Be(2);
        _mockRepo.Verify(r => r.UpdateAsync(shipment), Times.Once);
    }

    [Fact]
    public async Task AssignAsync_NonExistingShipment_ThrowsKeyNotFoundException()
    {
        var request = new AssignRequest { VehicleId = 1, DriverId = 2, ChangedBy = "operator" };

        _mockRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Shipment?)null);

        Func<Task> act = () => _service.AssignAsync(999, request);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Shipment with id 999 not found*");
    }
}
