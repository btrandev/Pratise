using System.ComponentModel.DataAnnotations;

namespace AdminService.Domain.Entities;

public class Tenant : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Domain { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    [MaxLength(20)]
    public string? SubscriptionPlan { get; set; }
    
    // Navigation properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
