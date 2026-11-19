
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wholesale.Web.Data;
using Wholesale.Web.Models;
using Wholesale.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace Wholesale.Web.Controllers
{
    [Authorize(Roles = "Agent")]
    public class CatalogController : Controller
    {
        private readonly AppDbContext _context;

        public CatalogController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _context.Set<Item>()
                .AsNoTracking()
                .OrderBy(i => i.ItemID)
                .Select(i => new ItemViewModel
                {
                    ItemID = i.ItemID,
                    ItemName = i.ItemName,
                    Size = i.Size,
                    Price = i.Price,
                    StockQuantity = i.StockQuantity
                })
                .ToListAsync();

            return View(items);
        }
    }
}
