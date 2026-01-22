using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EShop.Domain.DomainModels;
using EShop.Domain.DTO;
using EShop.Repository;

namespace EShop.Service
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IRepository<ShoppingCart> _shoppingCartRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductInShoppingCart> _productsInCartRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<ProductInOrder> _productsInOrderRepository;
        private readonly IEmailService _emailService;

        public ShoppingCartService(
            IRepository<ShoppingCart> shoppingCartRepository,
            IRepository<Product> productRepository,
            IRepository<ProductInShoppingCart> productsInCartRepository,
            IRepository<Order> orderRepository,
            IRepository<ProductInOrder> productsInOrderRepository,
            IEmailService emailService)

        {
            _shoppingCartRepository = shoppingCartRepository;
            _productRepository = productRepository;
            _productsInCartRepository = productsInCartRepository;
            _orderRepository = orderRepository;
            _productsInOrderRepository = productsInOrderRepository;
            _emailService = emailService;

        }

        public ShoppingCart GetByUserId(Guid userId)
        {
            var userIdString = userId.ToString();

            var cart = _shoppingCartRepository.Get(x => x.OwnerId == userIdString);

            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    Id = Guid.NewGuid(),
                    OwnerId = userIdString,
                    ProductInShoppingCarts = new List<ProductInShoppingCart>()
                };

                _shoppingCartRepository.Insert(cart);
            }

            return cart;
        }

        public ShoppingCartDTO GetByUserIdIncludingProducts(Guid userId)
        {
            var userIdString = userId.ToString();

            var cart = _shoppingCartRepository.Get(
                x => x.OwnerId == userIdString,
                includeProperties: "ProductInShoppingCarts,ProductInShoppingCarts.Product");

            if (cart == null || cart.ProductInShoppingCarts == null)
            {
                return new ShoppingCartDTO
                {
                    Products = new List<ProductInShoppingCart>(),
                    TotalPrice = 0
                };
            }

            var products = cart.ProductInShoppingCarts.ToList();

            double total = 0.0;
            foreach (var item in products)
            {
                var price = item.Product?.Price ?? 0;
                total += item.Quantity * price;
            }

            return new ShoppingCartDTO
            {
                Products = products,
                TotalPrice = total
            };
        }

        public AddToCartDTO GetProductInfo(Guid productId)
        {
            var product = _productRepository.Get(x => x.Id == productId);

            if (product == null)
            {
                return new AddToCartDTO
                {
                    ProductId = productId,
                    ProductName = "Unknown product",
                    Quantity = 1
                };
            }

            return new AddToCartDTO
            {
                ProductId = productId,
                ProductName = product.ProductName,
                Quantity = 1
            };
        }

        public bool DeleteFromCart(Guid productId, Guid userId)
        {
            var cart = GetByUserId(userId);
            if (cart == null) return false;

            var item = _productsInCartRepository.Get(
                x => x.ProductId == productId && x.ShoppingCartId == cart.Id);

            if (item == null) return false;

            _productsInCartRepository.Delete(item);
            return true;
        }

        public bool OrderProducts(Guid userId)
{
    // load cart with products and owner
    var shoppingCart = _shoppingCartRepository.Get(
        x => x.OwnerId == userId.ToString(),
        includeProperties: "ProductInShoppingCarts,ProductInShoppingCarts.Product,Owner");

    if (shoppingCart == null ||
        shoppingCart.ProductInShoppingCarts == null ||
        !shoppingCart.ProductInShoppingCarts.Any())
    {
        return false; // nothing to order
    }

    // 1) create order
    var newOrder = new Order
    {
        Id = Guid.NewGuid(),
        Owner = shoppingCart.Owner!,
        OwnerId = shoppingCart.OwnerId
    };
    _orderRepository.Insert(newOrder);

    // 2) move products from cart to order
    var productsInOrder = shoppingCart.ProductInShoppingCarts.Select(z => new ProductInOrder
    {
        Id = Guid.NewGuid(),
        Product = z.Product,
        ProductId = z.ProductId,
        Order = newOrder,
        OrderId = newOrder.Id,
        Quantity = z.Quantity
    }).ToList();

    double total = 0.0;

    foreach (var product in productsInOrder)
    {
        total += product.Quantity * (product.Product?.Price ?? 0);
        _productsInOrderRepository.Insert(product);
    }

    // 3) build confirmation email
    if (shoppingCart.Owner != null && !string.IsNullOrEmpty(shoppingCart.Owner.Email))
    {
        var sb = new StringBuilder();
        sb.AppendLine("Your order is completed. The order contains:\n");

        foreach (var p in productsInOrder)
        {
            var name = p.Product?.ProductName ?? "Unknown product";
            var price = p.Product?.Price ?? 0;
            sb.AppendLine($"{name} - quantity: {p.Quantity}, price: {price}");
        }

        sb.AppendLine();
        sb.AppendLine($"Total price for your order: {total}");

        var message = new EmailMessage
        {
            Id = Guid.NewGuid(),
            MailTo = shoppingCart.Owner.Email,
            Subject = "Successful order",
            Content = sb.ToString(),
            Status = false
        };

        var sent = _emailService.SendEmailAsync(message);
        message.Status = sent;
        // we’re not persisting EmailMessage to DB now; table is ready if you want later
    }

    // 4) clear cart
    shoppingCart.ProductInShoppingCarts.Clear();
    _shoppingCartRepository.Update(shoppingCart);

    return true;
}

    }
}
