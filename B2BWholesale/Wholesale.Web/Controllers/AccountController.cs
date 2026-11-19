
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Wholesale.Web.Data;
using Wholesale.Web.Models;
using Wholesale.Web.ViewModels;
using Wholesale.Web.Helpers;

namespace Wholesale.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;

        public AccountController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true &&
                User.IsInRole("Agent"))
            {
                return RedirectToAction("Index", "Catalog");
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string username = model.Username.Trim();

            var user = await _db.Set<User>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserName == username);

            if (user == null ||
                !SeedPasswordVerifier.Verify(model.Password, user.Password) ||
                user.Lock ||
                user.Role != "Agent" ||
                user.AgentID == null)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid username, password or account not allowed."
                );

                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("AgentID", user.AgentID.Value.ToString())
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
                }
            );

            return RedirectToAction("Index", "Catalog");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Denied()
        {
            return Content("Access denied. Agent account required.");
        }
    }
}
