using System;
using System.Collections.Generic;
using EShop.Domain.DomainModels;

namespace EShop.Service
{
    public interface IOrderService
    {
        List<Order> GetAllOrders();
        Order? GetOrderDetails(Guid id);
    }
}