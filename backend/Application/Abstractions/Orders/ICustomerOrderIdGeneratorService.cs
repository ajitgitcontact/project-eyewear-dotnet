namespace backend.Application.Abstractions.Orders;

public interface ICustomerOrderIdGeneratorService
{
    Task<string> GenerateAsync();
}
