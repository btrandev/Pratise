using System.ComponentModel.DataAnnotations;

namespace AdminService.Domain.Entities;

public abstract class BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public Guid CreatedById { get; set; }
    
    public Guid? UpdatedById { get; set; }
    
    // Optional: Keep string fields for display purposes if needed
    public string? CreatedByName { get; set; }
    
    public string? UpdatedByName { get; set; }
    
    public bool IsDeleted { get; set; }
}
