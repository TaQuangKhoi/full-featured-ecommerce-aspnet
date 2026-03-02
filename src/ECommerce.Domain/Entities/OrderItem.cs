using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Domain.Entities;

public class OrderItem : BaseEntity
{
    [Required]
    public Guid OrderId { get; set; }

    public virtual Order Order { get; set; } = null!;

    [Required]
    public Guid ProductId { get; set; }

    public virtual Product Product { get; set; } = null!;

    [Required]
    public int Quantity { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }
}
