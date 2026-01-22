using System;
using System.Collections.Generic;

namespace EShop.Domain.DTO
{
    public class OrderDTO
    {
        public Guid Id { get; set; }

        public string? OwnerId { get; set; }
        public string? UserEmail { get; set; }

        public List<ProductInOrderDTO> Products { get; set; } = new();

        public int TotalPrice { get; set; }
    }
}