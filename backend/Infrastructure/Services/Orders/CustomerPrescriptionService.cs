using backend.Application.Abstractions.Orders;
using backend.Application.Exceptions;
using backend.Data;
using backend.DTOs.CustomerPrescriptionDtos;
using backend.Models.Orders;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Services.Orders;

public class CustomerPrescriptionService : ICustomerPrescriptionService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CustomerPrescriptionService> _logger;

    public CustomerPrescriptionService(AppDbContext context, ILogger<CustomerPrescriptionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<CustomerPrescriptionResponseDto>> GetByCustomerOrderIdAsync(string customerOrderId)
    {
        _logger.LogInformation("Fetching customer prescriptions. Input: CustomerOrderId={CustomerOrderId}", customerOrderId);
        var prescriptions = await _context.CustomerPrescriptions
            .Where(cp => cp.CustomerOrderId == customerOrderId)
            .Select(cp => MapToDto(cp))
            .ToListAsync();
        _logger.LogInformation("Fetched customer prescriptions. Input: CustomerOrderId={CustomerOrderId} => Output: Count={Count}", customerOrderId, prescriptions.Count);
        return prescriptions;
    }

    public async Task<IEnumerable<CustomerPrescriptionResponseDto>> GetByUserIdAsync(int userId)
    {
        _logger.LogInformation("Fetching customer prescriptions by user. Input: UserId={UserId}", userId);
        var prescriptions = await _context.CustomerPrescriptions
            .Where(cp => cp.UserId == userId)
            .Select(cp => MapToDto(cp))
            .ToListAsync();
        _logger.LogInformation("Fetched customer prescriptions by user. Input: UserId={UserId} => Output: Count={Count}", userId, prescriptions.Count);
        return prescriptions;
    }

    public async Task<CustomerPrescriptionResponseDto> GetByIdAsync(string id)
    {
        _logger.LogInformation("Fetching customer prescription by id. Input: CustomerPrescriptionsId={CustomerPrescriptionsId}", id);
        var prescription = await _context.CustomerPrescriptions.FindAsync(id);
        if (prescription is null)
            throw new NotFoundException("Customer prescription not found.");

        return MapToDto(prescription);
    }

    public async Task<CustomerPrescriptionResponseDto> CreateAsync(CreateCustomerPrescriptionDto dto)
    {
        _logger.LogInformation("Creating customer prescription. Input: UserId={UserId}, CustomerOrderId={CustomerOrderId}", dto.UserId, dto.CustomerOrderId);
        await ValidateUserExistsAsync(dto.UserId);
        await ValidateOrderExistsAsync(dto.CustomerOrderId);

        var prescription = new CustomerPrescription
        {
            UserId = dto.UserId,
            CustomerOrderId = dto.CustomerOrderId,
            RightSphere = dto.RightSphere,
            RightCylinder = dto.RightCylinder,
            RightAxis = dto.RightAxis,
            RightAdd = dto.RightAdd,
            LeftSphere = dto.LeftSphere,
            LeftCylinder = dto.LeftCylinder,
            LeftAxis = dto.LeftAxis,
            LeftAdd = dto.LeftAdd,
            PD = dto.PD,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.CustomerPrescriptions.Add(prescription);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Customer prescription created. Output: CustomerPrescriptionsId={CustomerPrescriptionsId}", prescription.CustomerPrescriptionsId);
        return MapToDto(prescription);
    }

    public async Task<CustomerPrescriptionResponseDto> UpdateAsync(string id, UpdateCustomerPrescriptionDto dto)
    {
        _logger.LogInformation("Updating customer prescription. Input: CustomerPrescriptionsId={CustomerPrescriptionsId}", id);
        var prescription = await _context.CustomerPrescriptions.FindAsync(id);
        if (prescription is null)
            throw new NotFoundException("Customer prescription not found.");

        prescription.RightSphere = dto.RightSphere;
        prescription.RightCylinder = dto.RightCylinder;
        prescription.RightAxis = dto.RightAxis;
        prescription.RightAdd = dto.RightAdd;
        prescription.LeftSphere = dto.LeftSphere;
        prescription.LeftCylinder = dto.LeftCylinder;
        prescription.LeftAxis = dto.LeftAxis;
        prescription.LeftAdd = dto.LeftAdd;
        prescription.PD = dto.PD;
        prescription.Notes = dto.Notes;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Customer prescription updated. Input: CustomerPrescriptionsId={CustomerPrescriptionsId}", id);
        return MapToDto(prescription);
    }

    public async Task DeleteAsync(string id)
    {
        _logger.LogInformation("Deleting customer prescription. Input: CustomerPrescriptionsId={CustomerPrescriptionsId}", id);
        var prescription = await _context.CustomerPrescriptions.FindAsync(id);
        if (prescription is null)
            throw new NotFoundException("Customer prescription not found.");

        _context.CustomerPrescriptions.Remove(prescription);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Delete customer prescription result. Input: CustomerPrescriptionsId={CustomerPrescriptionsId} => Output: Deleted=true", id);
    }

    private async Task ValidateUserExistsAsync(int userId)
    {
        var exists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!exists)
            throw new NotFoundException("User not found.");
    }

    private async Task ValidateOrderExistsAsync(string customerOrderId)
    {
        var exists = await _context.Orders.AnyAsync(o => o.CustomerOrderId == customerOrderId);
        if (!exists)
            throw new NotFoundException("Order not found.");
    }

    private static CustomerPrescriptionResponseDto MapToDto(CustomerPrescription prescription)
    {
        return new CustomerPrescriptionResponseDto
        {
            CustomerPrescriptionsId = prescription.CustomerPrescriptionsId,
            UserId = prescription.UserId,
            CustomerOrderId = prescription.CustomerOrderId,
            RightSphere = prescription.RightSphere,
            RightCylinder = prescription.RightCylinder,
            RightAxis = prescription.RightAxis,
            RightAdd = prescription.RightAdd,
            LeftSphere = prescription.LeftSphere,
            LeftCylinder = prescription.LeftCylinder,
            LeftAxis = prescription.LeftAxis,
            LeftAdd = prescription.LeftAdd,
            PD = prescription.PD,
            Notes = prescription.Notes,
            CreatedAt = prescription.CreatedAt,
            UpdatedAt = prescription.UpdatedAt
        };
    }
}
