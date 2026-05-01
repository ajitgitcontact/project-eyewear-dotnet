namespace backend.DTOs.CustomerPrescriptionDtos;

public class CustomerPrescriptionResponseDto
{
    public string CustomerPrescriptionsId { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string CustomerOrderId { get; set; } = string.Empty;
    public decimal? RightSphere { get; set; }
    public decimal? RightCylinder { get; set; }
    public int? RightAxis { get; set; }
    public decimal? RightAdd { get; set; }
    public decimal? LeftSphere { get; set; }
    public decimal? LeftCylinder { get; set; }
    public int? LeftAxis { get; set; }
    public decimal? LeftAdd { get; set; }
    public decimal? PD { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
