using System.ComponentModel.DataAnnotations;

namespace CourierMax.Application.DTOs;

public class UpdateStatusRequest
{
    [Required(ErrorMessage = "New status is required")]
    public string NewStatus { get; set; } = string.Empty;

    public string? Reason { get; set; }

    [Required(ErrorMessage = "ChangedBy is required")]
    public string ChangedBy { get; set; } = string.Empty;
}
