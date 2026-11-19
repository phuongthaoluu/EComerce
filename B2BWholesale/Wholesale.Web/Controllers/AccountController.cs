
using Microsoft.AspNetCore.Mvc;
using Wholesale.Web.ViewModels;

namespace Wholesale.Web.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ViewBag.Message = "Database login is not connected yet.";
            return View(model);
        }
    }
}
