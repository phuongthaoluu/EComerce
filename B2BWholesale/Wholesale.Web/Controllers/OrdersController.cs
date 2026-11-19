
using Microsoft.AspNetCore.Mvc;
using Wholesale.Web.Services;
using Wholesale.Web.ViewModels;

namespace Wholesale.Web.Controllers
{
    public class OrdersController : Controller
    {
        [HttpGet]
        public IActionResult Create(int? itemId)
        {
            var items = SampleItemData.GetItems();

            var model = new CreateOrderViewModel();

            foreach (var item in items)
            {
                model.Lines.Add(new OrderLineViewModel
                {
                    ItemID = item.ItemID,
                    ItemName = item.ItemName,
                    Size = item.Size,
                    Price = item.Price,
                    StockQuantity = item.StockQuantity,
                    Quantity = item.ItemID == itemId ? 1 : 0
                });
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateOrderViewModel model)
        {
            var items = SampleItemData.GetItems();
            var selectedItems = new List<OrderLineViewModel>();
            var usedIds = new HashSet<int>();

            for (int i = 0; i < model.Lines.Count; i++)
            {
                var line = model.Lines[i];

                var item = items.FirstOrDefault(
                    x => x.ItemID == line.ItemID);

                if (item == null)
                {
                    ModelState.AddModelError(
                        "", "Invalid product.");
                    continue;
                }

                line.ItemName = item.ItemName;
                line.Size = item.Size;
                line.Price = item.Price;
                line.StockQuantity = item.StockQuantity;

                if (!usedIds.Add(line.ItemID))
                {
                    ModelState.AddModelError(
                        "", "Duplicate product.");
                    continue;
                }

                if (line.Quantity < 0)
                {
                    ModelState.AddModelError(
                        $"Lines[{i}].Quantity",
                        "Quantity cannot be negative.");
                    continue;
                }

                if (line.Quantity > item.StockQuantity)
                {
                    ModelState.AddModelError(
                        $"Lines[{i}].Quantity",
                        "Insufficient stock.");
                    continue;
                }

                if (line.Quantity > 0)
                {
                    selectedItems.Add(line);
                }
            }

            if (selectedItems.Count == 0)
            {
                ModelState.AddModelError(
                    "", "Please select at least one product.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            return View("Preview", selectedItems);
        }
    }
}
