namespace AdminService.Features.Users.UpdatePermissions;

public class UpdateUserPermissionsRequest
{
    public Guid UserId { get; set; }
    
    public string[] Permissions { get; set; } = Array.Empty<string>();
}
