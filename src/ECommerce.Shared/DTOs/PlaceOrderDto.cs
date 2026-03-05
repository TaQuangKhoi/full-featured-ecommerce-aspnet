using System.ComponentModel.DataAnnotations;

namespace ECommerce.Shared.DTOs;

/// <summary>
/// Request body for POST /api/orders (place order / checkout).
/// CustomerId is not included here — it is resolved from the authenticated user's claims.
/// </summary>
public class PlaceOrderDto
{
    [Required]
    [MaxLength(500)]
    public string ShippingAddress { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public List<CreateOrderItemDto> Items { get; set; } = new();
}
