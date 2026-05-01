using System.ComponentModel.DataAnnotations;
using backend.Models.Orders;

namespace backend.DTOs.CouponDtos;

public class CreateCouponDto
{
    [Required]
    [MaxLength(100)]
    public string CouponCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string CouponName { get; set; } = string.Empty;

    public DiscountValueType CouponType { get; set; }

    [Range(0, double.MaxValue)]
    public decimal CouponValue { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MinimumOrderAmount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MaximumCouponAmount { get; set; }

    [Range(1, int.MaxValue)]
    public int? UsageLimit { get; set; }

    [Range(1, int.MaxValue)]
    public int? PerUserUsageLimit { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;
}

public class UpdateCouponDto : CreateCouponDto
{
}

public class CouponResponseDto
{
    public string CouponId { get; set; } = string.Empty;
    public string CouponCode { get; set; } = string.Empty;
    public string CouponName { get; set; } = string.Empty;
    public DiscountValueType CouponType { get; set; }
    public decimal CouponValue { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public decimal? MaximumCouponAmount { get; set; }
    public int? UsageLimit { get; set; }
    public int? PerUserUsageLimit { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
