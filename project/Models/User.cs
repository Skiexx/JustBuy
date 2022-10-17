using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JustBuyApi.Models;

[Table("Users")]
public class User
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required] 
    public string? FullName { get; set; }
    
    [Required] 
    public string? Email { get; set; }
    
    [Required, MinLength(6)] 
    public string? Password { get; set; }
    
    [Required] 
    public int RoleId { get; set; }
    
    [ForeignKey("RoleId")]
    public Role? Role { get; set; }
}