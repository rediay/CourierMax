using Moq;
using FluentAssertions;
using CourierMax.Application.DTOs;
using CourierMax.Application.Services;
using CourierMax.Domain.Entities;
using CourierMax.Domain.Enums;
using CourierMax.Domain.Exceptions;
using CourierMax.Domain.Interfaces;
using CourierMax.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace CourierMax.Tests.Application.Services;

public class ShipmentServiceTests
{
    private readonly Mock<IShipmentRepository> _mockRepo;
    private readonly Mock<IVehicleRepository> _mockVehicleRepo;
    private readonly Mock<IDriverRepository> _mockDriverRepo;
    private readonly Mock<ICostCalculationService> _mockCost;
    private readonly ShipmentService _service;

    public ShipmentServiceTests()
    {
        _mockRepo = new Mock<IShipmentRepository>();
        _mockVehicleRepo = new Mock<IVehicleRepository>();
        _mockDriverRepo = new Mock<IDriverRepository>();
        _mockCost = new Mock<ICostCalculationService>();
        _service = new ShipmentService(
            _mockRepo.Object, _mockVehicleRepo.Object, _mockDriverRepo.Object, _mockCost.Object,
            Mock.Of<ILogger<ShipmentService>>());
    }

    private static Shipment CreateSampleShipment(decimal weightKg = 5, ServiceType serviceType = ServiceType.Estandar) =>
        new(
            "Juan Perez", new Phone("3001234567"), new Address("Calle 123"),
            "Maria Lopez", new Phone("3102345678"), new Address("Carrera 456"),
            new Weight(weightKg), new Dimensions(30, 20, 10),
            PackageType.Paquete, serviceType, "Bogotá", "Medellín");

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

        _mockRepo.Setup(r => r.TrackingCodeExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Shipment>())).Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(request);

        result.Should().NotBeNull();
        result.TrackingCode.Should().MatchRegex(@"^CM-\d{8}$");
        result.SenderName.Should().Be("Juan Perez");
        result.Status.Should().Be("CREADO");

        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Shipment>()), Times.Once);
    }

    [Theory]
    [InlineData("Quibdó", "Medellín")]
    [InlineData("Bogotá", "Leticia")]
    public async Task CreateAsync_InvalidCity_ThrowsArgumentException(string origin, string destination)
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
            Origin = origin,
            Destination = destination
        };

        Func<Task> act = () => _service.CreateAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Shipment>()), Times.Never);
    }

    [Fact]
    public async Task GetByTrackingCodeAsync_ExistingCode_ReturnsShipment()
    {
        var shipment = CreateSampleShipment();

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
    public async Task AssignAsync_ExplicitActiveDriverWithCapacity_AssignsAndLoadsVehicle()
    {
        var shipment = CreateSampleShipment(weightKg: 5);
        var driver = new Driver("Carlos Ruiz", null, null);
        var vehicle = new Vehicle("GHI-789", 1, 800, 15);
        var request = new AssignRequest { DriverId = 1, ChangedBy = "operator" };

        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(shipment);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Shipment>())).Returns(Task.CompletedTask);
        _mockDriverRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(driver);
        _mockVehicleRepo.Setup(r => r.GetByDriverIdAsync(0)).ReturnsAsync(vehicle);
        _mockCost.Setup(c => c.CalculateAsync("Bogotá", "Medellín", 5, ServiceType.Estandar, PackageType.Paquete))
            .ReturnsAsync(new CostEstimateResponse { TotalCost = 15000m });

        var result = await _service.AssignAsync(1, request);

        result.Should().NotBeNull();
        result.Status.Should().Be("ASIGNADO");
        result.DriverId.Should().Be(driver.Id);
        result.TotalCost.Should().Be(15000);
        vehicle.CurrentWeightKg.Should().Be(5);
        _mockVehicleRepo.Verify(r => r.UpdateAsync(vehicle), Times.Once);
    }

    [Fact]
    public async Task AssignAsync_ExplicitInactiveDriver_ThrowsInvalidOperationException()
    {
        var shipment = CreateSampleShipment();
        var driver = new Driver("Inactive Driver", null, null);
        typeof(Driver).GetProperty(nameof(Driver.IsActive))!.SetValue(driver, false);
        var request = new AssignRequest { DriverId = 1, ChangedBy = "operator" };

        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(shipment);
        _mockDriverRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(driver);

        Func<Task> act = () => _service.AssignAsync(1, request);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not active*");
    }

    [Fact]
    public async Task AssignAsync_ExplicitDriverVehicleExceedsCapacity_ThrowsInvalidOperationException()
    {
        var shipment = CreateSampleShipment(weightKg: 50);
        var driver = new Driver("María López", null, null);
        var vehicle = new Vehicle("DEF-456", 1, 40, 6); // max 40kg, shipment needs 50kg
        var request = new AssignRequest { DriverId = 1, ChangedBy = "operator" };

        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(shipment);
        _mockDriverRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(driver);
        _mockVehicleRepo.Setup(r => r.GetByDriverIdAsync(0)).ReturnsAsync(vehicle);

        Func<Task> act = () => _service.AssignAsync(1, request);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*capacity*");
    }

    [Fact]
    public async Task AssignAsync_NoDriverSpecified_SelectsVehicleWithLeastCurrentLoad()
    {
        var shipment = CreateSampleShipment(weightKg: 5);
        var request = new AssignRequest { ChangedBy = "operator" };

        var driverA = new Driver("Driver A", null, null);
        var driverB = new Driver("Driver B", null, null);
        var heavilyLoadedVehicle = new Vehicle("AAA-111", 0, 500, 10);
        heavilyLoadedVehicle.LoadCargo(400, 5);
        var lightlyLoadedVehicle = new Vehicle("BBB-222", 0, 500, 10);
        lightlyLoadedVehicle.LoadCargo(10, 0.1m);

        SetPrivateNavigation(heavilyLoadedVehicle, driverA);
        SetPrivateNavigation(lightlyLoadedVehicle, driverB);

        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(shipment);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Shipment>())).Returns(Task.CompletedTask);
        _mockVehicleRepo.Setup(r => r.GetAllWithActiveDriverAsync())
            .ReturnsAsync(new[] { heavilyLoadedVehicle, lightlyLoadedVehicle });
        _mockCost.Setup(c => c.CalculateAsync("Bogotá", "Medellín", 5, ServiceType.Estandar, PackageType.Paquete))
            .ReturnsAsync(new CostEstimateResponse { TotalCost = 13000m });

        var result = await _service.AssignAsync(1, request);

        result.DriverId.Should().Be(driverB.Id);
        _mockVehicleRepo.Verify(r => r.UpdateAsync(lightlyLoadedVehicle), Times.Once);
    }

    [Fact]
    public async Task AssignAsync_NonExistingShipment_ThrowsKeyNotFoundException()
    {
        var request = new AssignRequest { DriverId = 1, ChangedBy = "operator" };

        _mockRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Shipment?)null);

        Func<Task> act = () => _service.AssignAsync(999, request);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Shipment with id 999 not found*");
    }

    [Fact]
    public async Task UpdateStatusAsync_CancelAssignedShipment_ReleasesVehicleCapacity()
    {
        var shipment = CreateSampleShipment(weightKg: 10);
        var vehicle = new Vehicle("GHI-789", 1, 800, 15);
        vehicle.LoadCargo(10, shipment.PackageDimensions.VolumeM3);
        shipment.Assign(1, 1, "operator", 15000m);

        var request = new UpdateStatusRequest { NewStatus = "CANCELADO", Reason = "Cliente canceló", ChangedBy = "operator" };

        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(shipment);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Shipment>())).Returns(Task.CompletedTask);
        _mockVehicleRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(vehicle);
        _mockVehicleRepo.Setup(r => r.UpdateAsync(It.IsAny<Vehicle>())).Returns(Task.CompletedTask);

        var result = await _service.UpdateStatusAsync(1, request);

        result.Status.Should().Be("CANCELADO");
        vehicle.CurrentWeightKg.Should().Be(0);
        _mockVehicleRepo.Verify(r => r.UpdateAsync(vehicle), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_CancelDeliveredShipment_ThrowsShipmentStateConflictException()
    {
        var shipment = CreateSampleShipment();
        shipment.Assign(1, 1, "operator", 15000m);
        shipment.MarkInTransit("operator");
        shipment.Deliver("operator");

        var request = new UpdateStatusRequest { NewStatus = "CANCELADO", Reason = "Demasiado tarde", ChangedBy = "operator" };

        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(shipment);

        Func<Task> act = () => _service.UpdateStatusAsync(1, request);

        await act.Should().ThrowAsync<ShipmentStateConflictException>();
    }

    [Fact]
    public async Task GetCostEstimateAsync_ExistingShipment_ReturnsEstimate()
    {
        var shipment = CreateSampleShipment();
        var trackingCode = shipment.TrackingCode.ToString();
        var expectedEstimate = new CostEstimateResponse
        {
            Origin = "Bogotá",
            Destination = "Medellín",
            DistanceKm = 480,
            DistanceFee = 12000,
            BaseFee = 8000,
            WeightSurcharge = 4500,
            PackageSurcharge = 0,
            TotalCost = 24500
        };

        _mockRepo.Setup(r => r.GetByTrackingCodeAsync(trackingCode)).ReturnsAsync(shipment);
        _mockCost.Setup(c => c.CalculateAsync("Bogotá", "Medellín", 5, ServiceType.Estandar, PackageType.Paquete))
            .ReturnsAsync(expectedEstimate);

        var result = await _service.GetCostEstimateAsync(trackingCode);

        result.Should().BeSameAs(expectedEstimate);
    }

    [Fact]
    public async Task GetCostEstimateAsync_NonExistingShipment_ThrowsKeyNotFoundException()
    {
        _mockRepo.Setup(r => r.GetByTrackingCodeAsync("CM-99999999"))
            .ReturnsAsync((Shipment?)null);

        Func<Task> act = () => _service.GetCostEstimateAsync("CM-99999999");

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task GetOverdueShipmentsAsync_FiltersOnlyOverdueShipments()
    {
        var overdueShipment = CreateSampleShipment(serviceType: ServiceType.MismoDia);
        SetCreatedAt(overdueShipment, DateTime.UtcNow.AddDays(-3));

        var onTimeShipment = CreateSampleShipment(serviceType: ServiceType.Estandar);
        SetCreatedAt(onTimeShipment, DateTime.UtcNow);

        var from = DateTime.UtcNow.AddDays(-5);
        var to = DateTime.UtcNow.AddDays(1);

        _mockRepo.Setup(r => r.GetByCreatedDateRangeAsync(from, to))
            .ReturnsAsync(new[] { overdueShipment, onTimeShipment });

        var result = (await _service.GetOverdueShipmentsAsync(from, to)).ToList();

        result.Should().ContainSingle(s => s.TrackingCode == overdueShipment.TrackingCode.ToString());
    }

    private static void SetCreatedAt(Shipment shipment, DateTime createdAt) =>
        typeof(Shipment).GetProperty(nameof(Shipment.CreatedAt))!.SetValue(shipment, createdAt);

    private static void SetPrivateNavigation(Vehicle vehicle, Driver driver) =>
        typeof(Vehicle).GetProperty(nameof(Vehicle.Driver))!.SetValue(vehicle, driver);
}
