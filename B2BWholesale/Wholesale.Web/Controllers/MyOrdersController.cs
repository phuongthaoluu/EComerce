
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Wholesale.Web.Data;
using Wholesale.Web.ViewModels;

namespace Wholesale.Web.Controllers
{
    [Authorize(Roles = "Agent")]
    public class MyOrdersController : Controller
    {
        private readonly AppDbContext _context;

        public MyOrdersController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdText, out int userId))
            {
                return Forbid();
            }

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    u.UserID == userId &&
                    u.Role == "Agent" &&
                    !u.Lock);

            if (user == null || user.AgentID == null)
            {
                return Forbid();
            }

            int agentId = user.AgentID.Value;

            var orders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.AgentID == agentId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new MyOrderViewModel
                {
                    OrderID = o.OrderID,
                    OrderDate = o.OrderDate,
                    OrderStatus = o.OrderStatus,
                    TotalAmount = o.OrderDetails
                        .Sum(d => (decimal?)
                            (d.Quantity * d.UnitAmount)) ?? 0m
                })
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdText, out int userId))
            {
                return Forbid();
            }

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    u.UserID == userId &&
                    u.Role == "Agent" &&
                    !u.Lock);

            if (user == null || user.AgentID == null)
            {
                return Forbid();
            }

            int agentId = user.AgentID.Value;

            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Item)
                .FirstOrDefaultAsync(o =>
                    o.OrderID == id &&
                    o.AgentID == agentId);

            if (order == null)
            {
                return NotFound();
            }

            var items = order.OrderDetails
                .Select(d => new OrderItemViewModel
                {
                    ItemName = d.Item?.ItemName ?? "Unknown Item",
                    Quantity = d.Quantity,
                    UnitAmount = d.UnitAmount
                })
                .ToList();

            var model = new OrderDetailsViewModel
            {
                OrderID = order.OrderID,
                OrderDate = order.OrderDate,
                OrderStatus = order.OrderStatus,
                Items = items,
                TotalAmount = items.Sum(i => i.Amount)
            };

            return View(model);
        }
    }
}
