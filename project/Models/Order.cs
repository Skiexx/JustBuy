using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS1591

namespace JustBuyApi.Models;

[Table("Orders")]
[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
public class Order
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }
    
    [Required]
    public int UserId { get; init; }
    
    [Required] 
    public bool Payed { get; set; }
    
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    public virtual List<Cart>? Carts { get; set; }
}