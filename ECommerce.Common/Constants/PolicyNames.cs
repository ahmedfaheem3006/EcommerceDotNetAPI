namespace ECommerce.Common.Constants
{
    /// <summary>
    /// Policy names used for authorization
    /// </summary>
    public static class PolicyNames
    {
        public const string AdminOnly = "AdminOnly";
        public const string UserOnly = "UserOnly";
        public const string AdminOrManager = "AdminOrManager";
    }

    public static class RoleNames
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string User = "User";
    }
}
