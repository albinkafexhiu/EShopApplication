using System;

namespace EShop.Domain.DomainModels
{
    public class ProductInOrder : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public Guid OrderId { get; set; }
        public Order? Order { get; set; }

        public int Quantity { get; set; }
    }
}