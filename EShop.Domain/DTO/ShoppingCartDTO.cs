using System.Collections.Generic;
using EShop.Domain.DomainModels;

namespace EShop.Domain.DTO
{
    public class ShoppingCartDTO
    {
        public List<ProductInShoppingCart> Products { get; set; } = new List<ProductInShoppingCart>();

        public double TotalPrice { get; set; }
    }
}