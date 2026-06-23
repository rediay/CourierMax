using System.ComponentModel.DataAnnotations;

namespace CourierMax.Application.DTOs;

public class AssignRequest
{
    /// <summary>
    /// Optional. If omitted, the system selects an active driver whose vehicle has enough
    /// capacity and the least current load (load balancing per RN-01).
    /// </summary>
    public int? DriverId { get; set; }

    [Required(ErrorMessage = "ChangedBy is required")]
    public string ChangedBy { get; set; } = string.Empty;
}
