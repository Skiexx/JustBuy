using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JustBuyApi.Models;

[Table("Orders")]
public class Order
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required] 
    public bool Payed { get; set; }
    
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    public virtual List<Cart>? Carts { get; set; }
}