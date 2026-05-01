using NpgsqlTypes;

namespace backend.Models.Orders;

public enum PaymentStatus
{
    [PgName("PENDING")]
    PENDING,
    [PgName("PAID")]
    PAID,
    [PgName("FAILED")]
    FAILED
}

public enum OrderStatus
{
    [PgName("CREATED")]
    CREATED,
    [PgName("CONFIRMED")]
    CONFIRMED,
    [PgName("SHIPPED")]
    SHIPPED,
    [PgName("DELIVERED")]
    DELIVERED,
    [PgName("CANCELLED")]
    CANCELLED
}

public enum PaymentMethod
{
    [PgName("COD")]
    COD,
    [PgName("PREPAID")]
    PREPAID,
    [PgName("UPI")]
    UPI,
    [PgName("CARD")]
    CARD
}

public enum PaymentTxnStatus
{
    [PgName("INITIATED")]
    INITIATED,
    [PgName("SUCCESS")]
    SUCCESS,
    [PgName("FAILED")]
    FAILED
}

public enum AddressType
{
    [PgName("SHIPPING")]
    SHIPPING,
    [PgName("BILLING")]
    BILLING
}
