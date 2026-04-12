namespace backend.Constants;

public static class Roles
{
    public const string SuperAdmin = "SUPER_ADMIN";
    public const string Admin = "ADMIN";
    public const string Customer = "CUSTOMER";

    public enum RoleEnum
    {
        SUPER_ADMIN,
        ADMIN,
        CUSTOMER
    }
}
