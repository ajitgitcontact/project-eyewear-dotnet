using System.Globalization;
using backend.Application.Abstractions.Orders;
using backend.Application.Exceptions;
using backend.Data;
using backend.Models.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace backend.Infrastructure.Services.Orders;

public class CustomerOrderIdGeneratorService : ICustomerOrderIdGeneratorService
{
    private const int MaxDailySequence = 99999;

    private readonly AppDbContext _context;
    private readonly ILogger<CustomerOrderIdGeneratorService> _logger;

    public CustomerOrderIdGeneratorService(AppDbContext context, ILogger<CustomerOrderIdGeneratorService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string> GenerateAsync()
    {
        var sequenceDate = DateOnly.FromDateTime(DateTime.Now);
        _logger.LogInformation("Reading/updating order number sequence. SequenceDate={SequenceDate}", sequenceDate);

        var nextSequence = await GetNextSequenceAsync(sequenceDate);

        if (nextSequence > MaxDailySequence)
        {
            _logger.LogError("Daily order number sequence exhausted. SequenceDate={SequenceDate}, Sequence={Sequence}", sequenceDate, nextSequence);
            throw new ConflictException("Daily order number sequence limit reached.");
        }

        var customerOrderId = $"{sequenceDate.ToString("yyMMdd", CultureInfo.InvariantCulture)}{nextSequence:D5}";
        _logger.LogInformation("Generated customer order id. SequenceDate={SequenceDate}, Sequence={Sequence}, CustomerOrderId={CustomerOrderId}", sequenceDate, nextSequence, customerOrderId);

        return customerOrderId;
    }

    private async Task<int> GetNextSequenceAsync(DateOnly sequenceDate)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.Transaction = _context.Database.CurrentTransaction?.GetDbTransaction();
        command.CommandText = """
            INSERT INTO "OrderNumberSequences" ("OrderNumberSequencesId", "SequenceDate", "LastSequenceNumber", "CreatedAt", "UpdatedAt")
            VALUES (@sequenceId, @sequenceDate, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
            ON CONFLICT ("SequenceDate")
            DO UPDATE SET
                "LastSequenceNumber" = "OrderNumberSequences"."LastSequenceNumber" + 1,
                "UpdatedAt" = CURRENT_TIMESTAMP
            RETURNING "LastSequenceNumber";
            """;

        var sequenceIdParameter = command.CreateParameter();
        sequenceIdParameter.ParameterName = "sequenceId";
        sequenceIdParameter.Value = PrefixedId.Create("order_number_sequences");
        command.Parameters.Add(sequenceIdParameter);

        var sequenceDateParameter = command.CreateParameter();
        sequenceDateParameter.ParameterName = "sequenceDate";
        sequenceDateParameter.Value = sequenceDate;
        command.Parameters.Add(sequenceDateParameter);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
}
