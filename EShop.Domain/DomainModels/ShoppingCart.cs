using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EShop.Domain.Identity;

namespace EShop.Domain.DomainModels
{
    public class ShoppingCart : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        public string? OwnerId { get; set; }
        public EShopApplicationUser? Owner { get; set; }

        public virtual ICollection<ProductInShoppingCart>? ProductInShoppingCarts { get; set; }
    }
}