namespace CourierMax.Domain.Exceptions;

/// <summary>
/// Thrown when an operation conflicts with the shipment's current state
/// (e.g. assigning a shipment that is already assigned, cancelling one
/// already delivered). Maps to HTTP 409 Conflict.
/// </summary>
public class ShipmentStateConflictException : Exception
{
    public ShipmentStateConflictException(string message) : base(message)
    {
    }
}
