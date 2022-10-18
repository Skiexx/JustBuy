using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JustBuyApi.Models;

[Table("Carts")]
public class Cart
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    public int Quantity { get; set; }
    
    [Required]
    public int OrderId { get; set; }
    
    [ForeignKey("ProductId")]
    public virtual Product? Product { get; set; }
    
    [ForeignKey("OrderId")]
    public virtual Order? Order { get; set; }
}