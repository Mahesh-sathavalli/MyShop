using MyShop.Core.Contracts;
using MyShop.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyShop.Core.ViewModels;

namespace MyShop.Services
{
    public class OrderService : IOrderService
    {
        IRepository<Order> orderContext;
        public OrderService(IRepository<Order> OrderContext) {
            this.orderContext = OrderContext;
        }

        public void CreateOrder(Order baseOrder, List<BasketItemViewModel> basketItems)
        {
            foreach (var item in basketItems) {
                baseOrder.OrderItems.Add(new OrderItem()
                {
                    ProductId = item.ProductID,
                    Image = item.Image,
                    Price = item.DiscountedPrice,
                    ProductName = item.ProductName,
                    Quanity = item.Quanity
                });
            }

            orderContext.Insert(baseOrder);
            orderContext.Commit();
        }

        public List<Order> GetOrderList() {
            return orderContext.Collection().OrderByDescending(a=>a.CreatedAt).ToList();
        }

        public Order GetOrder(string Id) {
            return orderContext.Find(Id);
        }

        public void UpdateOrder(Order updatedOrder) {
            orderContext.Update(updatedOrder);
            orderContext.Commit();
        }
    }
}
