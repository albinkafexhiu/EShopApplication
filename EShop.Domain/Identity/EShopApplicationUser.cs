using System.Collections.Generic;
using EShop.Domain.DomainModels;
using Microsoft.AspNetCore.Identity;

namespace EShop.Domain.Identity
{
    public class EShopApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Address { get; set; }

        public ShoppingCart? UserCart { get; set; }
        public virtual ICollection<Product>? UserProducts { get; set; }

        // NEW: orders for this user
        public virtual ICollection<Order>? Orders { get; set; }
    }
}