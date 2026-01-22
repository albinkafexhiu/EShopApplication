using System;
using System.Collections.Generic;
using System.Linq;
using EShop.Domain.DomainModels;
using EShop.Repository;

namespace EShop.Service
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<Order> _orderRepository;

        public OrderService(IRepository<Order> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public List<Order> GetAllOrders()
        {
            // include Owner and ProductsInOrder -> Product
            return _orderRepository
                .GetAll(includeProperties: "Owner,ProductsInOrder,ProductsInOrder.Product")
                .ToList();
        }

        public Order? GetOrderDetails(Guid id)
        {
            return _orderRepository.Get(
                o => o.Id == id,
                includeProperties: "Owner,ProductsInOrder,ProductsInOrder.Product"
            );
        }
    }
}