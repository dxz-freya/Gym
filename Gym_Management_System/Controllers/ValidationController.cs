using Microsoft.AspNetCore.Mvc;
using System.Linq;
using GymManagement.Data;
using GymManagement.Models;
using Microsoft.AspNetCore.Identity;

public class ValidationController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;

    public ValidationController(AppDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // ✅ 用于注册页面：检查邮箱是否重复
    [AcceptVerbs("Get", "Post")]
    public JsonResult CheckEmail(string email)
    {
        var exists = _context.Users.Any(u => u.Email == email);
        if (exists)
        {
            return Json($"Email '{email}' is already registered.");
        }
        return Json(true);
    }

    // ✅ 用于注册页面：检查用户名是否重复
    [AcceptVerbs("Get", "Post")]
    public JsonResult CheckUsername(string username)
    {
        var exists = _context.Users.Any(u => u.UserName == username);
        if (exists)
        {
            return Json($"Username '{username}' is already taken.");
        }
        return Json(true);
    }

    // ✅ 用于 EditProfile 页面：只要这个 email 不是自己的，就报错
    [AcceptVerbs("Get", "Post")]
    public async Task<IActionResult> CheckEmailEdit(string email)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json("User not found.");
        }

        var exists = _context.Users.Any(u => u.Email == email && u.Id != currentUser.Id);
        if (exists)
        {
            return Json($"Email '{email}' is already registered.");
        }

        return Json(true);
    }
}
