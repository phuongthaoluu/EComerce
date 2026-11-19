
using Microsoft.AspNetCore.Mvc;
using Wholesale.Web.Services;

namespace Wholesale.Web.Controllers
{
    public class CatalogController : Controller
    {
        public IActionResult Index()
        {
            var items = SampleItemData.GetItems();

            return View(items);
        }
    }
}
