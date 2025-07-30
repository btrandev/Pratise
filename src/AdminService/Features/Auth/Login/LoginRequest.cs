using Common.Middleware.Attributes;

namespace AdminService.Features.Auth.Login;

public class LoginRequest
{
    public string Email { get; set; }
    
    [Sensitive] // Marking password as sensitive for logging purposes
    public string Password { get; set; }
    
    public Guid? TenantId { get; set; }
}
