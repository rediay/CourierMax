using System.ComponentModel.DataAnnotations;

namespace CourierMax.Application.DTOs;

public class AssignRequest
{
    [Required(ErrorMessage = "VehicleId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "VehicleId must be greater than 0")]
    public int VehicleId { get; set; }

    [Required(ErrorMessage = "DriverId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "DriverId must be greater than 0")]
    public int DriverId { get; set; }

    [Required(ErrorMessage = "ChangedBy is required")]
    public string ChangedBy { get; set; } = string.Empty;
}
