using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using GymManagement.Models;

namespace GymManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<HomeController> _logger;

        public HomeController(UserManager<User> userManager, ILogger<HomeController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }


        // 首页
        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Admin"))
                    return RedirectToAction("Dashboard", "Admin", new { area = "Admin" });

                if (roles.Contains("Trainer"))
                    return RedirectToAction("Dashboard", "Trainer");

                if (roles.Contains("Receptionist"))
                    return RedirectToAction("Dashboard", "Receptionist");

                if (roles.Contains("Customer"))
                    return RedirectToAction("Dashboard", "Customer");
            }

            return View(); // fallback for not logged-in
        }

        // 课程时间表页
        [HttpGet]
        public IActionResult ClassSchedule()
        {
            _logger.LogInformation("Visited Class Schedule page");
            return View();
        }

        // 会员方案页
        [HttpGet]
        public IActionResult MembershipOptions()
        {
            _logger.LogInformation("Visited Membership Options page");
            return View();
        }

        // 联系我们页（GET）
        [HttpGet]
        public IActionResult Contact()
        {
            _logger.LogInformation("Visited Contact page");
            return View();
        }

        // 联系我们页（POST）
        [HttpPost]
        public IActionResult Contact(string Name, string Email, string Subject, string Message)
        {
            _logger.LogInformation("Contact form submitted by {Name} ({Email})", Name, Email);

            TempData["ToastMessage"] = $"Thank you, {Name}! Your message has been received.";
            TempData["ToastClass"] = "success";
            TempData["ToastTitle"] = "Message Sent";

            return RedirectToAction("Contact");
        }

        // 错误页
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            _logger.LogError("Unhandled error occurred");
            return View();
        }
    }
}
