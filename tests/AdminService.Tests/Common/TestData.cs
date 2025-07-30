using AdminService.Domain.Entities;
using Bogus;

namespace AdminService.Tests.Common;

/// <summary>
/// Utility class to generate test data
/// </summary>
public static class TestData
{
    private static readonly Faker _faker = new Faker("en");
    
    public static Tenant GenerateTenant(Guid? id = null)
    {
        return new Tenant
        {
            Id = id ?? Guid.NewGuid(),
            Name = _faker.Company.CompanyName(),
            Code = _faker.Random.AlphaNumeric(6).ToUpper(),
            Description = _faker.Company.CatchPhrase(),
            Domain = _faker.Internet.DomainName(),
            SubscriptionPlan = _faker.PickRandom("Free", "Standard", "Enterprise"),
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedById = Guid.NewGuid(),
            CreatedByName = _faker.Name.FullName()
        };
    }
    
    public static User GenerateUser(Guid? id = null, Guid? tenantId = null)
    {
        var userId = id ?? Guid.NewGuid();
        var firstName = _faker.Name.FirstName();
        var lastName = _faker.Name.LastName();
        
        return new User
        {
            Id = userId,
            FirstName = firstName,
            LastName = lastName,
            Email = _faker.Internet.Email(firstName, lastName),
            Username = _faker.Internet.UserName(firstName, lastName),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            PhoneNumber = _faker.Phone.PhoneNumber(),
            Role = _faker.PickRandom("Admin", "TenantAdmin", "User"),
            IsActive = true,
            EmailConfirmed = true,
            TenantId = tenantId ?? Guid.NewGuid(),
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedById = Guid.NewGuid(),
            CreatedByName = _faker.Name.FullName(),
            Claims = new List<UserClaim>()
        };
    }
    
    public static UserClaim GenerateUserClaim(Guid? id = null, Guid? userId = null, string claimType = null, string claimValue = null)
    {
        return new UserClaim
        {
            Id = id ?? Guid.NewGuid(),
            ClaimType = claimType ?? "permission",
            ClaimValue = claimValue ?? _faker.PickRandom("Users.View", "Users.Create", "Users.Update", "Users.Delete"),
            UserId = userId ?? Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            CreatedById = Guid.NewGuid(),
            CreatedByName = _faker.Name.FullName()
        };
    }
    
    public static List<UserClaim> GeneratePermissionClaims(Guid userId, string[] permissions)
    {
        var claims = new List<UserClaim>();
        
        foreach (var permission in permissions)
        {
            claims.Add(new UserClaim
            {
                Id = Guid.NewGuid(),
                ClaimType = "permission",
                ClaimValue = permission,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                CreatedById = Guid.NewGuid(),
                CreatedByName = "Test System"
            });
        }
        
        return claims;
    }
}
