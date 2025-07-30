# Permission-Based Authorization System

## Overview

The authorization system in the Admin Service API uses a permission-based approach integrated with MediatR pipeline behaviors. This enables centralized permission checks for all incoming requests, reducing code duplication and ensuring consistent authorization throughout the application.

## Key Components

### 1. Permission Constants

Permissions are defined as constants in the `Permissions` class:

```csharp
public static class Permissions
{
    public static class Users
    {
        public const string View = "Users.View";
        public const string Create = "Users.Create";
        public const string Update = "Users.Update";
        public const string Delete = "Users.Delete";
    }
    
    public static class Tenants
    {
        public const string View = "Tenants.View";
        public const string Create = "Tenants.Create";
        public const string Update = "Tenants.Update";
        public const string Delete = "Tenants.Delete";
    }
}
```

### 2. Role-Based Permission Sets

Common role-based permission sets are also defined in the `Permissions` class:

```csharp
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
```

### 3. MediatR Authorization Behavior

The `PermissionRequirementBehavior` is a MediatR pipeline behavior that intercepts all requests and checks if they require authorization. If a request implements `IRequireAuthorization`, it will verify that the user has the necessary permissions.

### 4. Marking Requests for Authorization

To require authorization for a request, implement the `IRequireAuthorization` interface and specify the required permissions:

```csharp
public record GetUserQuery(Guid UserId) : IRequest<Result<UserResponse>>, IRequireAuthorization
{
    public string[] RequiredPermissions => new[] { Permissions.Users.View };
}
```

You can also use the `[Authorize]` attribute:

```csharp
[Authorize(Permissions.Users.View)]
public record GetUserQuery(Guid UserId) : IRequest<Result<UserResponse>>, IRequireAuthorization
{
    public string[] RequiredPermissions => new[] { Permissions.Users.View };
}
```

### 5. User Claims for Permissions

Permissions are stored as claims in the `UserClaim` table with `ClaimType="permission"` and `ClaimValue` set to the permission string.

### 6. JWT Tokens with Permission Claims

When a user logs in, their permissions are added to the JWT token as claims, enabling efficient permission checks without database queries for each request.

## API Endpoints

### Managing Permissions

1. **Get User Permissions**
   - Endpoint: `GET /api/users/{userId}/permissions`
   - Requires: `Users.View` permission
   - Returns: List of permissions assigned to the user

2. **Update User Permissions**
   - Endpoint: `PUT /api/users/{userId}/permissions`
   - Requires: `Users.Update` permission
   - Body: `{ "permissions": ["Users.View", "Tenants.View"] }`
   - Updates user's permissions to match the provided list

## Permission Manager Utility

The `PermissionManager` utility class provides methods to simplify permission management:

- `SyncPermissionsForUserAsync` - Syncs a user's permissions based on their role
- `AddPermissionToUserAsync` - Adds a specific permission to a user
- `RemovePermissionFromUserAsync` - Removes a specific permission from a user

## Usage Examples

### Check for Permission in a Handler

```csharp
if (!_currentUser.HasPermission(Permissions.Users.Update))
{
    return Result<bool>.Failure("You don't have permission to update this user.");
}
```

### Check for Multiple Permissions

```csharp
if (!_currentUser.HasPermissions(new[] { Permissions.Users.View, Permissions.Tenants.View }))
{
    return Result<bool>.Failure("You don't have the required permissions.");
}
```

## Initial Setup

The system includes a migration to seed an initial admin user with all permissions. You can use this account to manage other users and assign permissions.

- Username: `admin`
- Password: `admin123` (change this in production)
