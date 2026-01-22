using System;

namespace EShop.Domain.DTO
{
    public class AddToCartDTO
    {
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
    }
}