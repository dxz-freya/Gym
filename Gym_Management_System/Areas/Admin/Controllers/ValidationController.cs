using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GymManagement.Models;

namespace GymManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ValidationController : Controller
    {
        private readonly UserManager<User> _userManager;

        public ValidationController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> CheckUsername(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user != null)
            {
                return Json($"This username '{username}' is already taken.");
            }
            return Json(true);
        }

        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> CheckEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                return Json($"This email '{email}' is already registered.");
            }
            return Json(true);
        }

        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> CheckEmailEdit(string contact, string id)
        {
            var user = await _userManager.FindByEmailAsync(contact);
            return Json(user == null || user.Id == id ? true : $"The email '{contact}' is already registered by another user.");
        }
    }
}
