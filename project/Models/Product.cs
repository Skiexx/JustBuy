using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS1591

namespace JustBuyApi.Models;

[Table("Products")]
[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
public class Product
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }
    
    [Required] 
    public string? Name { get; set; }
    
    [Required] 
    public string? Description { get; set; }
    
    [Required] 
    public int Price { get; set; }
    
    public virtual List<Cart>? Carts { get; set; }
}