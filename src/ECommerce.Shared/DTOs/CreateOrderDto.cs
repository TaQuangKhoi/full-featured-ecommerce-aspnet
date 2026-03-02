using System.ComponentModel.DataAnnotations;

namespace ECommerce.Shared.DTOs;

public class CreateOrderDto
{
    [Required]
    public string CustomerId { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string ShippingAddress { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public List<CreateOrderItemDto> Items { get; set; } = new();
}
