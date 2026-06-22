using CourierMax.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CourierMax.Application.DTOs;

public class CreateShipmentRequest
{
    [Required(ErrorMessage = "Sender name is required")]
    [MaxLength(100)]
    public string SenderName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sender phone is required")]
    public string SenderPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sender address is required")]
    [MaxLength(200)]
    public string SenderAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Recipient name is required")]
    [MaxLength(100)]
    public string RecipientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Recipient phone is required")]
    public string RecipientPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Recipient address is required")]
    [MaxLength(200)]
    public string RecipientAddress { get; set; } = string.Empty;

    [Range(0.1, 100, ErrorMessage = "Weight must be between 0.1 and 100 kg")]
    public decimal PackageWeight { get; set; }

    [Range(1, 200, ErrorMessage = "Length must be between 1 and 200 cm")]
    public decimal PackageLength { get; set; }

    [Range(1, 200, ErrorMessage = "Width must be between 1 and 200 cm")]
    public decimal PackageWidth { get; set; }

    [Range(1, 200, ErrorMessage = "Height must be between 1 and 200 cm")]
    public decimal PackageHeight { get; set; }

    [Required]
    public PackageType PackageType { get; set; }

    [Required]
    public ServiceType ServiceType { get; set; }

    [Required(ErrorMessage = "Origin city is required")]
    public string Origin { get; set; } = string.Empty;

    [Required(ErrorMessage = "Destination city is required")]
    public string Destination { get; set; } = string.Empty;
}
