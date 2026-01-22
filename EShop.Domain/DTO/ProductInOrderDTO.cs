using System;

namespace EShop.Domain.DTO
{
    public class ProductInOrderDTO
    {
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }

        public int Price { get; set; }     
        public int Quantity { get; set; }

        // Price * Quantity (calculated when mapping)
        public int LineTotal { get; set; }
    }
}