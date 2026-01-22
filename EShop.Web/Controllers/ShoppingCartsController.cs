using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EShop.Repository.Data;
using EShop.Service;

namespace EShop.Web.Controllers
{
    [Authorize]
    public class ShoppingCartsController : Controller
    {
        private readonly IShoppingCartService _cartService;
        private readonly ApplicationDbContext _context;

        public ShoppingCartsController(
            IShoppingCartService cartService,
            ApplicationDbContext context)
        {
            _cartService = cartService;
            _context = context;
        }

        public IActionResult Index()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return Challenge();
            }

            var userId = Guid.Parse(userIdString);
            var cartDto = _cartService.GetByUserIdIncludingProducts(userId);

            return View(cartDto);
        }

        public IActionResult DeleteFromCart(Guid id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return Challenge();
            }

            var userId = Guid.Parse(userIdString);
            _cartService.DeleteFromCart(id, userId);

            return RedirectToAction("Index");
        }

        public IActionResult Order()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return Challenge();
            }

            var userId = Guid.Parse(userIdString);
            _cartService.OrderProducts(userId);

            return RedirectToAction("Index");
        }

        // 🔥 DEBUG: see all orders directly in browser as JSON
        [HttpGet]
        public IActionResult OrdersDebug()
        {
            var data = _context.Orders
                .Include(o => o.ProductsInOrder)
                .ThenInclude(pio => pio.Product)
                .Select(o => new
                {
                    o.Id,
                    o.OwnerId,
                    Products = o.ProductsInOrder.Select(pio => new
                    {
                        pio.ProductId,
                        ProductName = pio.Product.ProductName,
                        pio.Quantity,
                        Price = pio.Product.Price,
                        LineTotal = pio.Quantity * pio.Product.Price
                    }),
                })
                .ToList();

            return Json(data);
        }

    }
}
