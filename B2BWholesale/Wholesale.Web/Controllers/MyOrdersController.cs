
using Microsoft.AspNetCore.Mvc;
using Wholesale.Web.ViewModels;

namespace Wholesale.Web.Controllers
{
    public class MyOrdersController : Controller
    {
        public IActionResult Index()
        {
            var orders = new List<MyOrderViewModel>
            {
                new MyOrderViewModel
                {
                    OrderID = 1001,
                    OrderDate = new DateTime(2026, 11, 15),
                    OrderStatus = "Pending",
                    TotalAmount = 195000
                },
                new MyOrderViewModel
                {
                    OrderID = 1002,
                    OrderDate = new DateTime(2026, 11, 16),
                    OrderStatus = "Shipped",
                    TotalAmount = 350000
                }
            };

            return View(orders);
        }


        public IActionResult Details(int id)
        {
            if (id != 1001 && id != 1002)
            {
                return NotFound();
            }

            var order = new OrderDetailsViewModel
            {
                OrderID = id,
                OrderDate = id == 1001
                    ? new DateTime(2026, 11, 15)
                    : new DateTime(2026, 11, 16),
                OrderStatus = id == 1001 ? "Pending" : "Shipped"
            };

            if (id == 1001)
            {
                order.Items = new List<OrderItemViewModel>
        {
            new OrderItemViewModel
            {
                ItemName = "Coca-Cola",
                Quantity = 10,
                UnitAmount = 12000
            },
            new OrderItemViewModel
            {
                ItemName = "Pepsi",
                Quantity = 5,
                UnitAmount = 15000
            }
        };
            }
            else
            {
                order.Items = new List<OrderItemViewModel>
        {
            new OrderItemViewModel
            {
                ItemName = "Bottled Water",
                Quantity = 20,
                UnitAmount = 10000
            },
            new OrderItemViewModel
            {
                ItemName = "Orange Juice",
                Quantity = 10,
                UnitAmount = 15000
            }
        };
            }

            order.TotalAmount = order.Items.Sum(x => x.Amount);

            return View(order);
        }

    }
}