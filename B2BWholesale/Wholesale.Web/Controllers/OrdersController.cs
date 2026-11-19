using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wholesale.Web.Data;
using Wholesale.Web.Models;
using Wholesale.Web.ViewModels;

namespace Wholesale.Web.Controllers
{
    [Authorize(Roles = "Agent")]
    public class OrdersController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _environment;

        public OrdersController(
            AppDbContext db,
            IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? itemId)
        {
            if (!_environment.IsDevelopment())
                return NotFound();

            var model = await LoadFormAsync(null, itemId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderViewModel model)
        {
            if (!_environment.IsDevelopment())
                return NotFound();

            if (!ModelState.IsValid)
                return View(await LoadFormAsync(model.Lines));

            var selectedItems = await ValidateOrderAsync(model);

            if (selectedItems == null)
                return View(await LoadFormAsync(model.Lines));

            return View("Preview", selectedItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(CreateOrderViewModel model)
        {
            if (!_environment.IsDevelopment())
                return NotFound();

            if (!ModelState.IsValid)
            {
                TempData["OrderError"] = "Invalid order data.";
                return RedirectToAction(nameof(Create));
            }

            var selectedItems = await ValidateOrderAsync(model);

            if (selectedItems == null)
            {
                TempData["OrderError"] =
                    "Order is invalid or stock is insufficient.";

                return RedirectToAction(nameof(Create));
            }

            if (!int.TryParse(
                User.FindFirst("AgentID")?.Value,
                out int agentId))
            {
                return Forbid();
            }


            bool agentActive = await _db.Set<User>()
                .AsNoTracking()
                .AnyAsync(u =>
                    u.AgentID == agentId &&
                    u.Role == "Agent" &&
                    !u.Lock);

            if (!agentActive)
            {
                return Forbid();
            }



            await using var transaction =
                await _db.Database.BeginTransactionAsync();

            try
            {
                var order = new Wholesale.Web.Models.Order
                {
                    AgentID = agentId,
                    OrderDate = DateTime.Now,
                    OrderStatus = "Pending"
                };

                _db.Set<Wholesale.Web.Models.Order>().Add(order);

                await _db.SaveChangesAsync();

                foreach (var item in selectedItems)
                {
                    int updated = await _db.Database
                        .ExecuteSqlInterpolatedAsync($@"
                            UPDATE dbo.Item
                            SET StockQuantity = StockQuantity - {item.Quantity}
                            WHERE ItemID = {item.ItemID}
                            AND StockQuantity >= {item.Quantity}
                        ");

                    if (updated != 1)
                    {
                        await transaction.RollbackAsync();

                        TempData["OrderError"] =
                            "Insufficient stock for " + item.ItemName;

                        return RedirectToAction(nameof(Create));
                    }

                    var detail = new OrderDetail
                    {
                        OrderID = order.OrderID,
                        ItemID = item.ItemID,
                        Quantity = item.Quantity,
                        UnitAmount = item.Price
                    };

                    _db.Set<OrderDetail>().Add(detail);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(
                    nameof(Success),
                    new { id = order.OrderID });
            }
            catch
            {
                await transaction.RollbackAsync();

                TempData["OrderError"] =
                    "Cannot save order. Please try again.";

                return RedirectToAction(nameof(Create));
            }
        }

        [HttpGet]
        public IActionResult Success(int id)
        {
            if (!_environment.IsDevelopment())
                return NotFound();

            return View(id);
        }

        private async Task<CreateOrderViewModel> LoadFormAsync(
            List<OrderLineViewModel>? postedLines = null,
            int? itemId = null)
        {
            var quantities = postedLines?
                .GroupBy(x => x.ItemID)
                .ToDictionary(x => x.Key, x => x.First().Quantity);

            var items = await _db.Set<Item>()
                .AsNoTracking()
                .OrderBy(x => x.ItemID)
                .ToListAsync();

            var model = new CreateOrderViewModel();

            foreach (var item in items)
            {
                int quantity = 0;

                if (quantities != null &&
                    quantities.TryGetValue(item.ItemID, out int savedQty))
                {
                    quantity = savedQty;
                }
                else if (item.ItemID == itemId)
                {
                    quantity = 1;
                }

                model.Lines.Add(new OrderLineViewModel
                {
                    ItemID = item.ItemID,
                    ItemName = item.ItemName,
                    Size = item.Size,
                    Price = item.Price,
                    StockQuantity = item.StockQuantity,
                    Quantity = quantity
                });
            }

            return model;
        }

        private async Task<List<OrderLineViewModel>?> ValidateOrderAsync(
            CreateOrderViewModel model)
        {
            if (model.Lines == null || model.Lines.Count == 0)
            {
                ModelState.AddModelError(
                    "", "Please select a product.");

                return null;
            }

            if (model.Lines.Any(x => x.Quantity < 0))
            {
                ModelState.AddModelError(
                    "", "Quantity cannot be negative.");

                return null;
            }

            var selected = model.Lines
                .Where(x => x.Quantity > 0)
                .ToList();

            if (selected.Count == 0)
            {
                ModelState.AddModelError(
                    "", "Please select at least one product.");

                return null;
            }

            if (selected.GroupBy(x => x.ItemID)
                .Any(x => x.Count() > 1))
            {
                ModelState.AddModelError(
                    "", "Duplicate product.");

                return null;
            }

            var ids = selected.Select(x => x.ItemID).ToList();

            var items = await _db.Set<Item>()
                .AsNoTracking()
                .Where(x => ids.Contains(x.ItemID))
                .ToDictionaryAsync(x => x.ItemID);

            var result = new List<OrderLineViewModel>();

            foreach (var line in selected)
            {
                if (!items.TryGetValue(line.ItemID, out var item))
                {
                    ModelState.AddModelError(
                        "", "Invalid product.");

                    return null;
                }

                if (line.Quantity > item.StockQuantity)
                {
                    ModelState.AddModelError(
                        "", "Insufficient stock for " + item.ItemName);

                    return null;
                }

                result.Add(new OrderLineViewModel
                {
                    ItemID = item.ItemID,
                    ItemName = item.ItemName,
                    Size = item.Size,
                    Price = item.Price,
                    StockQuantity = item.StockQuantity,
                    Quantity = line.Quantity
                });
            }

            return result;
        }
    }
}
