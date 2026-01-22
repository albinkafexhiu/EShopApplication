using System;
using System.Collections.Generic;
using System.Linq;
using EShop.Domain.DomainModels;
using EShop.Domain.DTO;
using EShop.Repository;

namespace EShop.Service
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductInShoppingCart> _productInCartRepository;
        private readonly IShoppingCartService _shoppingCartService;

        public ProductService(
            IRepository<Product> productRepository,
            IRepository<ProductInShoppingCart> productInCartRepository,
            IShoppingCartService shoppingCartService)
        {
            _productRepository = productRepository;
            _productInCartRepository = productInCartRepository;
            _shoppingCartService = shoppingCartService;
        }

        public List<Product> GetAllProducts()
        {
            return _productRepository.GetAll().ToList();
        }

        public Product? GetProductById(Guid id)
        {
            return _productRepository.Get(x => x.Id == id);
        }

        public Product CreateNewProduct(Product product)
        {
            return _productRepository.Insert(product);
        }

        public Product UpdateProduct(Product product)
        {
            return _productRepository.Update(product);
        }

        public Product? DeleteProduct(Guid id)
        {
            var product = GetProductById(id);
            if (product == null) return null;

            _productRepository.Delete(product);
            return product;
        }

        // SIMPLE: old version – adds quantity 1
        public void AddToCart(Guid productId, Guid userId)
        {
            var dto = new AddToCartDTO
            {
                ProductId = productId,
                Quantity = 1
            };

            AddToCart(dto, userId);
        }

        // NEW: version using DTO (supports custom quantity)
        public void AddToCart(AddToCartDTO model, Guid userId)
        {
            var cart = _shoppingCartService.GetByUserId(userId);
            if (cart == null) return;

            var product = _productRepository.Get(p => p.Id == model.ProductId);
            if (product == null) return;

            var existing = _productInCartRepository.Get(
                x => x.ProductId == model.ProductId && x.ShoppingCartId == cart.Id);

            var quantityToAdd = model.Quantity < 1 ? 1 : model.Quantity;

            if (existing == null)
            {
                var newItem = new ProductInShoppingCart
                {
                    Id = Guid.NewGuid(),
                    ProductId = model.ProductId,
                    Product = product,
                    ShoppingCartId = cart.Id,
                    ShoppingCart = cart,
                    Quantity = quantityToAdd
                };

                _productInCartRepository.Insert(newItem);
            }
            else
            {
                existing.Quantity += quantityToAdd;
                _productInCartRepository.Update(existing);
            }
        }
    }
}
