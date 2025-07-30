namespace AdminService.Common.Authorization;

/// <summary>
/// Constants for permission names
/// </summary>
public static class Permissions
{
    // User permissions
    public static class Users
    {
        public const string View = "Users.View";
        public const string Create = "Users.Create";
        public const string Update = "Users.Update";
        public const string Delete = "Users.Delete";
    }

    // Tenant permissions
    public static class Tenants
    {
        public const string View = "Tenants.View";
        public const string Create = "Tenants.Create";
        public const string Update = "Tenants.Update";
        public const string Delete = "Tenants.Delete";
    }

    // Role-based permission sets
    public static class Roles
    {
        public static readonly string[] Admin = 
        {
            Users.View, Users.Create, Users.Update, Users.Delete,
            Tenants.View, Tenants.Create, Tenants.Update, Tenants.Delete
        };

        public static readonly string[] TenantAdmin = 
        {
            Users.View, Users.Create, Users.Update,
            Tenants.View
        };

        public static readonly string[] StandardUser = 
        {
            Users.View,
            Tenants.View
        };
    }
}
