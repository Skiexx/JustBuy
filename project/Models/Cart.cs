using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS1591

namespace JustBuyApi.Models;

[Table("Carts")]
[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
public class Cart
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }
    
    [Required]
    public int ProductId { get; init; }
    
    [Required]
    public int Quantity { get; set; }
    
    [Required]
    public int OrderId { get; init; }
    
    [ForeignKey("ProductId")]
    public virtual Product? Product { get; set; }
    
    [ForeignKey("OrderId")]
    public virtual Order? Order { get; set; }
}