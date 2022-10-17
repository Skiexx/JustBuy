using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JustBuyApi.Models;

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
    public Product? Product { get; set; }
    
    [ForeignKey("OrderId")]
    public Order? Order { get; set; }
}