using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GymManagement.Models;
using GymManagement.ViewModels;
using GymManagement.Data;
using System.Security.Claims;

namespace GymManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IWebHostEnvironment _env;
        private readonly AppDbContext _dbContext;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IWebHostEnvironment env,
            AppDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _env = env;
            _dbContext = dbContext;
        }

        private async Task<IActionResult> RedirectToDashboardByRole(User user)
        {
            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return RedirectToAction("Dashboard", "Admin", new { area = "Admin" });
            if (await _userManager.IsInRoleAsync(user, "Trainer"))
                return RedirectToAction("Dashboard", "Trainer");
            if (await _userManager.IsInRoleAsync(user, "Receptionist"))
                return RedirectToAction("Dashboard", "Receptionist");
            if (await _userManager.IsInRoleAsync(user, "Customer"))
                return RedirectToAction("Dashboard", "Customer");

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var existingUserByUsername = await _userManager.FindByNameAsync(model.Username);
            if (existingUserByUsername != null)
            {
                ModelState.AddModelError("Username", "This username is already taken.");
                return View(model);
            }

            var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            var user = new Customer
            {
                UserName = model.Username,
                Email = model.Email,
                Name = model.Name,
                JoinDate = DateTime.UtcNow,
                DOB = model.DOB,
                PhoneNumber = "",
                MembershipType = "Basic",
                MembershipStatus = "Active",
                SubscriptionDate = DateTime.Now,
                MembershipExpiry = DateTime.Now.AddMonths(1),
                GymBranchId = 1
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Customer");
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Dashboard", "Customer");
            }

            foreach (var error in result.Errors)
            {
                if (error.Code.Contains("Password"))
                    ModelState.AddModelError("Password", error.Description);
                else if (error.Code.Contains("Email"))
                    ModelState.AddModelError("Email", error.Description);
                else if (error.Code.Contains("UserName"))
                    ModelState.AddModelError("Username", error.Description);
                else
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = "")
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user != null)
                    return await RedirectToDashboardByRole(user);
            }

            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
