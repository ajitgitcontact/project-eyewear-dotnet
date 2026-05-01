namespace backend.Models.Orders;

public static class PrefixedId
{
    public static string Create(string prefix)
    {
        return $"{prefix}_{Guid.NewGuid():N}";
    }
}
