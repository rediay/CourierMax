using CourierMax.Application.DTOs;
using CourierMax.Domain.Enums;

namespace CourierMax.Application.Services;

public interface ICostCalculationService
{
    Task<CostEstimateResponse> CalculateAsync(string origin, string destination, decimal weightKg, ServiceType serviceType, PackageType packageType);
}
