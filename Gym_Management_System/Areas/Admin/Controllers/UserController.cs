using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

using GymManagement.Services;
using GymManagement.ViewModels;
using GymManagement.Models;
using GymManagement.Helpers;
using GymManagement.Areas.Admin.Models; // 👈 确保有这个命名空间


namespace GymManagement.Areas.Admin.Controllers
{
  [Authorize(Roles = "Admin")]
  [Area("Admin")]
  public class UserController : Controller
  {
    private readonly UserManager<User> userManager;
    private readonly RoleManager<IdentityRole> roleManager;
    private readonly UserService _userService;

    public UserController(
        UserManager<User> userMgr,
        RoleManager<IdentityRole> roleMgr,
        UserService userService)
    {
      userManager = userMgr;
      roleManager = roleMgr;
      _userService = userService;
    }


    public async Task<IActionResult> Index(int page = 1)
    {
      const int PageSize = 7;

      var allUsers = userManager.Users;
      var usersPage = allUsers
          .OrderBy(u => u.UserName)
          .Skip((page - 1) * PageSize)
          .Take(PageSize)
          .ToList();

      foreach (var user in usersPage)
      {
        user.RoleNames = await userManager.GetRolesAsync(user);
      }

      var model = new UserViewModel
      {
        Users = usersPage,
        Roles = roleManager.Roles,
        PagingInfo = new PagingInfo
        {
          CurrentPage = page,
          ItemsPerPage = PageSize,
          TotalItems = allUsers.Count()
        }
      };

      return View(model);
    }



    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
      var user = await userManager.FindByIdAsync(id);
      if (user != null)
      {
        var roles = await userManager.GetRolesAsync(user);
        bool hasOnlyAdminRole = roles.Count == 1 && roles.Contains("Admin");

        if (hasOnlyAdminRole)
        {
          ToastHelper.ShowToast(this, "Cannot delete a user with only Admin role.", "warning");
          return RedirectToAction("Index");
        }

        var result = await userManager.DeleteAsync(user);
        if (result.Succeeded)
          ToastHelper.ShowToast(this, "User deleted successfully.", "success");
        else
          ToastHelper.ShowToast(this, string.Join(" | ", result.Errors.Select(e => e.Description)), "danger", "Failed");
      }
      else
      {
        ToastHelper.ShowToast(this, "User not found.", "danger", "Error");
      }

      return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> AddToAdmin(string id)
    {
      var user = await userManager.FindByIdAsync(id);
      if (user == null)
      {
        ToastHelper.ShowToast(this, "User not found.", "danger", "Error");
        return RedirectToAction("Index");
      }

      if (!await roleManager.RoleExistsAsync("Admin"))
      {
        ToastHelper.ShowToast(this, "Admin role does not exist.", "danger", "Error");
        return RedirectToAction("Index");
      }

      if (await userManager.IsInRoleAsync(user, "Admin"))
      {
        ToastHelper.ShowToast(this, "User is already an Admin.", "warning");
      }
      else
      {
        await userManager.AddToRoleAsync(user, "Admin");
        ToastHelper.ShowToast(this, "User promoted to Admin.", "success");
      }

      return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> RemoveFromAdmin(string id)
    {
      var user = await userManager.FindByIdAsync(id);
      if (user == null)
      {
        ToastHelper.ShowToast(this, "User not found.", "danger");
        return RedirectToAction("Index");
      }

      var roles = await userManager.GetRolesAsync(user);
      bool hasOnlyAdminRole = roles.Count == 1 && roles.Contains("Admin");

      if (hasOnlyAdminRole)
      {
        ToastHelper.ShowToast(this, "Cannot remove Admin role when it's the user's only role.", "warning");
        return RedirectToAction("Index");
      }

      if (!await userManager.IsInRoleAsync(user, "Admin"))
      {
        ToastHelper.ShowToast(this, "User is not an Admin.", "warning");
      }
      else
      {
        await userManager.RemoveFromRoleAsync(user, "Admin");
        ToastHelper.ShowToast(this, "Admin role removed.", "success");
      }

      return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteRole(string id)
    {
      var role = await roleManager.FindByIdAsync(id);
      if (role != null)
      {
        await roleManager.DeleteAsync(role);
        ToastHelper.ShowToast(this, "Role deleted.", "success");
      }
      else
      {
        ToastHelper.ShowToast(this, "Role not found.", "danger");
      }

      return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> CreateAdminRole()
    {
      var result = await roleManager.CreateAsync(new IdentityRole("Admin"));
      if (result.Succeeded)
        ToastHelper.ShowToast(this, "Admin role created.", "success");
      else
        ToastHelper.ShowToast(this, string.Join(" | ", result.Errors.Select(e => e.Description)), "danger");

      return RedirectToAction("Index");
    }


    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
      var user = await userManager.FindByIdAsync(id);
      if (user == null)
        return NotFound();

      var roles = await userManager.GetRolesAsync(user);

      var model = new UserEditViewModel
      {
        Id = user.Id,
        Username = user.UserName,
        FullName = user.Name,       // ✅ updated
        Contact = user.Email,       // ✅ updated
        SelectedRole = roles.FirstOrDefault(r => r != "Admin"),
        AvailableRoles = await roleManager.Roles
                              .Where(r => r.Name != "Admin")
                              .Select(r => r.Name)
                              .ToListAsync()
      };

      return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(UserEditViewModel model)
    {
      if (!ModelState.IsValid)
      {
        // 如果验证失败，重新加载角色列表以供下拉菜单显示
        model.AvailableRoles = await roleManager.Roles
            .Where(r => r.Name != "Admin")
            .Select(r => r.Name)
            .ToListAsync();

        return View(model); // 返回页面，显示错误信息
      }

      var user = await userManager.FindByIdAsync(model.Id);
      if (user == null)
        return NotFound();

      user.Name = model.FullName;
      user.Email = model.Contact;

      var currentRoles = await userManager.GetRolesAsync(user);
      if (!string.IsNullOrEmpty(model.SelectedRole) && model.SelectedRole != currentRoles.FirstOrDefault(r => r != "Admin"))
      {
        var rolesToRemove = currentRoles.Where(r => r != "Admin");
        await userManager.RemoveFromRolesAsync(user, rolesToRemove);
        await userManager.AddToRoleAsync(user, model.SelectedRole);
      }

      var result = await userManager.UpdateAsync(user);
      if (result.Succeeded)
      {
        ToastHelper.ShowToast(this, "User updated successfully.", "success");
        return RedirectToAction("Index");
      }

      // 如果数据库更新失败，也加上错误
      foreach (var error in result.Errors)
        ModelState.AddModelError("", error.Description);

      // 更新失败也要重新载入 role 列表
      model.AvailableRoles = await roleManager.Roles
          .Where(r => r.Name != "Admin")
          .Select(r => r.Name)
          .ToListAsync();

      return View(model);
    }



    [HttpGet]
    public IActionResult Add()
    {
      var model = new AddUserViewModel
      {
        AvailableRoles = roleManager.Roles
              .Where(r => r.Name != "Admin")
              .Select(r => r.Name)
              .ToList()
      };

      return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Add(AddUserViewModel model)
    {
      model.AvailableRoles = roleManager.Roles
          .Where(r => r.Name != "Admin")
          .Select(r => r.Name)
          .ToList();

      if (!ModelState.IsValid)
        return View(model);

      if (await userManager.FindByEmailAsync(model.Email) != null)
        ModelState.AddModelError("Email", "This email is already registered.");

      if (await userManager.FindByNameAsync(model.Username) != null)
        ModelState.AddModelError("Username", "This username is already taken.");

      if (!ModelState.IsValid)
        return View(model);

      var user = new User
      {
        UserName = model.Username,
        Email = model.Email,
        Name = model.Name
      };

      var result = await userManager.CreateAsync(user, model.Password);
      if (!result.Succeeded)
      {
        foreach (var error in result.Errors)
          ModelState.AddModelError("", error.Description);
        return View(model);
      }

      await userManager.AddToRoleAsync(user, model.SelectedRole);

      ToastHelper.ShowToast(this, "User created successfully.", "success");
      return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Search(string keyword)
    {
      var users = await _userService.GetUsersFilteredAsync(keyword);

      foreach (var user in users)
      {
        user.RoleNames = await userManager.GetRolesAsync(user);
      }

      var model = new UserViewModel
      {
        Users = users,
        Roles = await roleManager.Roles.ToListAsync()
      };

      return PartialView("_UserTableBody", model); // ✅ 只返回匹配项
    }








  }
}
