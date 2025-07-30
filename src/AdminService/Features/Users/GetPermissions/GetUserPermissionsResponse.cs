namespace AdminService.Features.Users.GetPermissions;

public class GetUserPermissionsResponse
{
    public Guid UserId { get; set; }
    public string[] Permissions { get; set; } = Array.Empty<string>();
}
