using System.Collections.Generic;
using EShop.Domain.Identity;

namespace EShop.Domain.DomainModels
{
    public class Order : BaseEntity
    {
        public string? OwnerId { get; set; }
        public EShopApplicationUser? Owner { get; set; }

        public virtual ICollection<ProductInOrder>? ProductsInOrder { get; set; }
    }
}