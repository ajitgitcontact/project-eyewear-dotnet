using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.CustomerPrescriptionDtos;

public class UpdateCustomerPrescriptionDto
{
    public decimal? RightSphere { get; set; }
    public decimal? RightCylinder { get; set; }
    [Range(0, 180)]
    public int? RightAxis { get; set; }
    public decimal? RightAdd { get; set; }
    public decimal? LeftSphere { get; set; }
    public decimal? LeftCylinder { get; set; }
    [Range(0, 180)]
    public int? LeftAxis { get; set; }
    public decimal? LeftAdd { get; set; }
    [Range(0, double.MaxValue)]
    public decimal? PD { get; set; }
    public string? Notes { get; set; }
}
