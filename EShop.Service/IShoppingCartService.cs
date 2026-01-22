using System;
using EShop.Domain.DomainModels;
using EShop.Domain.DTO;

namespace EShop.Service
{
    public interface IShoppingCartService
    {
        ShoppingCart GetByUserId(Guid userId);
        ShoppingCartDTO GetByUserIdIncludingProducts(Guid userId);

        AddToCartDTO GetProductInfo(Guid productId);
        bool DeleteFromCart(Guid productId, Guid userId);
        bool OrderProducts(Guid userId);
    }
}