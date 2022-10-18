using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JustBuyApi.Models;

[Table("Products")]
public class Product
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required] 
    public string? Name { get; set; }
    
    [Required] 
    public string? Description { get; set; }
    
    [Required] 
    public int Price { get; set; }
    
    public virtual List<Cart>? Carts { get; set; }
}