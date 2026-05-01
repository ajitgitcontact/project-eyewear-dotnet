using System.ComponentModel.DataAnnotations;
using backend.Models.Orders;

namespace backend.DTOs.DiscountDtos;

public class CreateDiscountDto
{
    [Required]
    [MaxLength(150)]
    public string DiscountName { get; set; } = string.Empty;

    public DiscountValueType DiscountType { get; set; }

    [Range(0, double.MaxValue)]
    public decimal DiscountValue { get; set; }

    public DiscountAppliesTo AppliesTo { get; set; } = DiscountAppliesTo.ALL;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public List<int> ProductIds { get; set; } = new();
}

public class UpdateDiscountDto : CreateDiscountDto
{
}

public class DiscountResponseDto
{
    public string DiscountId { get; set; } = string.Empty;
    public string DiscountName { get; set; } = string.Empty;
    public DiscountValueType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public DiscountAppliesTo AppliesTo { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public List<int> ProductIds { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
