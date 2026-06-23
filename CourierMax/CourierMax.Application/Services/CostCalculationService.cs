using CourierMax.Application.DTOs;
using CourierMax.Domain.Enums;
using CourierMax.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CourierMax.Application.Services;

public class CostCalculationService : ICostCalculationService
{
    private readonly ICityDistanceRepository _cityDistanceRepository;
    private readonly ILogger<CostCalculationService> _logger;

    private const decimal FreeWeightKg = 2m;
    private const decimal WeightSurchargePerKg = 1500m;

    private static readonly Dictionary<ServiceType, decimal> BaseFees = new()
    {
        [ServiceType.Estandar] = 8000m,
        [ServiceType.Express] = 15000m,
        [ServiceType.MismoDia] = 25000m
    };

    private static readonly Dictionary<PackageType, decimal> PackageSurchargeRates = new()
    {
        [PackageType.Documento] = 0m,
        [PackageType.Paquete] = 0m,
        [PackageType.Fragil] = 0.30m,
        [PackageType.Perecedero] = 0.25m
    };

    public CostCalculationService(ICityDistanceRepository cityDistanceRepository, ILogger<CostCalculationService> logger)
    {
        _cityDistanceRepository = cityDistanceRepository;
        _logger = logger;
    }

    public async Task<CostEstimateResponse> CalculateAsync(string origin, string destination, decimal weightKg, ServiceType serviceType, PackageType packageType)
    {
        var route = await _cityDistanceRepository.GetByRouteAsync(origin, destination)
            ?? throw new KeyNotFoundException($"No route found between '{origin}' and '{destination}'.");

        var baseFee = BaseFees.GetValueOrDefault(serviceType);
        var extraWeightKg = Math.Max(0, weightKg - FreeWeightKg);
        var weightSurcharge = extraWeightKg * WeightSurchargePerKg;
        var distanceFee = route.DistanceFee;

        var subtotal = baseFee + weightSurcharge + distanceFee;
        var packageSurchargeRate = PackageSurchargeRates.GetValueOrDefault(packageType);
        var packageSurcharge = Math.Round(subtotal * packageSurchargeRate, 2);

        var total = subtotal + packageSurcharge;

        _logger.LogInformation(
            "Cost calculated for {Origin}->{Destination} ({ServiceType}, {PackageType}, {WeightKg}kg): {TotalCost}",
            origin, destination, serviceType, packageType, weightKg, total);

        return new CostEstimateResponse
        {
            Origin = origin,
            Destination = destination,
            DistanceKm = route.DistanceKm,
            BaseFee = baseFee,
            WeightSurcharge = weightSurcharge,
            DistanceFee = distanceFee,
            PackageSurcharge = packageSurcharge,
            TotalCost = Math.Round(total, 2)
        };
    }
}
