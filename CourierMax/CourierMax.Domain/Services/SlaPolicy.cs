using CourierMax.Domain.Entities;
using CourierMax.Domain.Enums;

namespace CourierMax.Domain.Services;

public static class SlaPolicy
{
    public static readonly Dictionary<ServiceType, int> BusinessDaysByServiceType = new()
    {
        [ServiceType.Estandar] = 5,
        [ServiceType.Express] = 2,
        [ServiceType.MismoDia] = 0
    };

    public static int GetSlaBusinessDays(ServiceType serviceType) => BusinessDaysByServiceType[serviceType];

    /// <summary>
    /// A shipment is overdue when more business days than its SLA have elapsed since creation
    /// without reaching ENTREGADO. Cancelled shipments are never overdue.
    /// </summary>
    public static bool IsOverdue(Shipment shipment, DateTime asOf)
    {
        if (shipment.Status == ShipmentStatus.CANCELADO)
            return false;

        var slaDays = GetSlaBusinessDays(shipment.ServiceType);

        var referenceDate = shipment.Status == ShipmentStatus.ENTREGADO
            ? shipment.StatusHistories.FirstOrDefault(h => h.NewStatus == ShipmentStatus.ENTREGADO)?.ChangedAt ?? asOf
            : asOf;

        var elapsedBusinessDays = ColombianBusinessCalendar.CountBusinessDays(shipment.CreatedAt, referenceDate);

        return elapsedBusinessDays > slaDays;
    }
}
