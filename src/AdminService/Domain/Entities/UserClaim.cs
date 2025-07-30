using System.ComponentModel.DataAnnotations;

namespace AdminService.Domain.Entities;

public class UserClaim : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string ClaimType { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string ClaimValue { get; set; } = string.Empty;
    
    // Foreign key
    public Guid UserId { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
}
