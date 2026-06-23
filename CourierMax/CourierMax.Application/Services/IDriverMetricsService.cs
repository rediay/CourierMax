using CourierMax.Application.DTOs;

namespace CourierMax.Application.Services;

public interface IDriverMetricsService
{
    Task<DriverEfficiencyReportResponse> GetEfficiencyReportAsync(int driverId);
}
