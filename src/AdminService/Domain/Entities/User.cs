using System.ComponentModel.DataAnnotations;

namespace AdminService.Domain.Entities;

public class User : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(150)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public bool EmailConfirmed { get; set; } = false;
    
    public DateTime? LastLoginAt { get; set; }
    
    [MaxLength(20)]
    public string? Role { get; set; }
    
    // Foreign key
    public Guid TenantId { get; set; }
    
    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    
    public virtual ICollection<UserClaim> Claims { get; set; } = new List<UserClaim>();
}
