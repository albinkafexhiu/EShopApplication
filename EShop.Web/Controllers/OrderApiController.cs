using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using EShop.Service;

namespace EShop.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderApiController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderApiController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: /api/OrderApi
        [HttpGet]
        public IActionResult GetAll()
        {
            var orders = _orderService.GetAllOrders();

            // project to a simple shape so we don't get JSON cycles
            var result = orders.Select(o => new
            {
                o.Id,
                o.OwnerId,
                UserEmail = o.Owner?.Email,
                ProductsCount = o.ProductsInOrder?.Sum(pi => pi.Quantity) ?? 0,
                TotalPrice = o.ProductsInOrder?
                    .Sum(pi => pi.Quantity * (pi.Product?.Price ?? 0)) ?? 0
            });

            return Ok(result);
        }

        // GET: /api/OrderApi/{id}
        [HttpGet("{id:guid}")]
        public IActionResult GetById(Guid id)
        {
            var order = _orderService.GetOrderDetails(id);
            if (order == null)
                return NotFound();

            var result = new
            {
                order.Id,
                order.OwnerId,
                UserEmail = order.Owner?.Email,
                Products = order.ProductsInOrder?.Select(pi => new
                {
                    pi.ProductId,
                    ProductName = pi.Product?.ProductName,
                    Price = pi.Product?.Price ?? 0,
                    pi.Quantity,
                    LineTotal = pi.Quantity * (pi.Product?.Price ?? 0)
                }),
                TotalPrice = order.ProductsInOrder?
                    .Sum(pi => pi.Quantity * (pi.Product?.Price ?? 0)) ?? 0
            };

            return Ok(result);
        }
    }
}