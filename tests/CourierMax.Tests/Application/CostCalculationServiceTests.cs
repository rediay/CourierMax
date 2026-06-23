using Moq;
using FluentAssertions;
using CourierMax.Application.Services;
using CourierMax.Domain.Entities;
using CourierMax.Domain.Enums;
using CourierMax.Domain.Interfaces;

namespace CourierMax.Tests.Application.Services;

public class CostCalculationServiceTests
{
    private readonly Mock<ICityDistanceRepository> _mockRepo;
    private readonly CostCalculationService _service;

    public CostCalculationServiceTests()
    {
        _mockRepo = new Mock<ICityDistanceRepository>();
        _service = new CostCalculationService(_mockRepo.Object);
    }

    [Fact]
    public async Task CalculateAsync_FragilExpressBogotaMedellin_MatchesSpecExample()
    {
        // Base express: 15000
        // Peso extra: (5-2)kg * 1500 = 4500
        // Distancia Bogotá-Medellín: 12000
        // Recargo frágil: (15000+4500+12000) * 30% = 9450
        // Total: 40950
        var route = new CityDistance("Bogotá", "Medellín", 480, 12000);
        _mockRepo.Setup(r => r.GetByRouteAsync("Bogotá", "Medellín")).ReturnsAsync(route);

        var result = await _service.CalculateAsync("Bogotá", "Medellín", 5, ServiceType.Express, PackageType.Fragil);

        result.BaseFee.Should().Be(15000);
        result.WeightSurcharge.Should().Be(4500);
        result.DistanceFee.Should().Be(12000);
        result.PackageSurcharge.Should().Be(9450);
        result.TotalCost.Should().Be(40950);
    }

    [Fact]
    public async Task CalculateAsync_StandardDocumentoNoExtraWeight_NoSurcharges()
    {
        var route = new CityDistance("Bogotá", "Cali", 360, 9000);
        _mockRepo.Setup(r => r.GetByRouteAsync("Bogotá", "Cali")).ReturnsAsync(route);

        var result = await _service.CalculateAsync("Bogotá", "Cali", 1.5m, ServiceType.Estandar, PackageType.Documento);

        // Base estándar: 8000, sin peso extra (<=2kg), sin recargo de paquete
        result.WeightSurcharge.Should().Be(0);
        result.PackageSurcharge.Should().Be(0);
        result.TotalCost.Should().Be(8000 + 9000);
    }

    [Fact]
    public async Task CalculateAsync_PerecederoMismoDia_AppliesQuarterSurcharge()
    {
        var route = new CityDistance("Medellín", "Cali", 310, 8000);
        _mockRepo.Setup(r => r.GetByRouteAsync("Medellín", "Cali")).ReturnsAsync(route);

        var result = await _service.CalculateAsync("Medellín", "Cali", 2, ServiceType.MismoDia, PackageType.Perecedero);

        // Base mismo día: 25000, sin peso extra, distancia 8000
        // Recargo perecedero: (25000+0+8000) * 25% = 8250
        result.PackageSurcharge.Should().Be(8250);
        result.TotalCost.Should().Be(25000 + 8000 + 8250);
    }

    [Fact]
    public async Task CalculateAsync_ReverseRoute_UsesSameDistanceFee()
    {
        var route = new CityDistance("Bogotá", "Medellín", 480, 12000);
        _mockRepo.Setup(r => r.GetByRouteAsync("Medellín", "Bogotá")).ReturnsAsync(route);

        var result = await _service.CalculateAsync("Medellín", "Bogotá", 1, ServiceType.Estandar, PackageType.Paquete);

        result.DistanceFee.Should().Be(12000);
        result.TotalCost.Should().Be(8000 + 12000);
    }

    [Fact]
    public async Task CalculateAsync_NonExistingRoute_ThrowsKeyNotFoundException()
    {
        _mockRepo.Setup(r => r.GetByRouteAsync("Unknown", "Nowhere"))
            .ReturnsAsync((CityDistance?)null);

        Func<Task> act = () => _service.CalculateAsync("Unknown", "Nowhere", 1, ServiceType.Estandar, PackageType.Paquete);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*No route found*");
    }
}
