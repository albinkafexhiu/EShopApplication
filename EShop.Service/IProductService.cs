using System;
using System.Collections.Generic;
using EShop.Domain.DomainModels;
using EShop.Domain.DTO;

namespace EShop.Service
{
    public interface IProductService
    {
        List<Product> GetAllProducts();
        Product? GetProductById(Guid id);
        Product CreateNewProduct(Product product);
        Product UpdateProduct(Product product);
        Product? DeleteProduct(Guid id);

        // you already had this (simple add: +1)
        void AddToCart(Guid productId, Guid userId);

        // NEW: add with DTO (supports Quantity from form)
        void AddToCart(AddToCartDTO model, Guid userId);
    }
}