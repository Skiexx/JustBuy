using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS1591

namespace JustBuyApi.Models;

[Table("Users")]
[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
public class User
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }
    
    [Required] 
    public string? FullName { get; set; }
    
    [Required] 
    public string? Email { get; init; }
    
    [Required, MinLength(6)] 
    public string? Password { get; init; }
    
    [Required] 
    public int RoleId { get; init; }
    
    [ForeignKey("RoleId")]
    public virtual Role? Role { get; set; }
    
    public virtual List<Order>? Orders { get; set; }
}