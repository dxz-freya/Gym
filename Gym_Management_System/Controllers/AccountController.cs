using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GymManagement.Models;
using GymManagement.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace GymManagement.Controllers
{
  public class AccountController : Controller
  {
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IWebHostEnvironment _env;

    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, IWebHostEnvironment env)
    {
      _userManager = userManager;
      _signInManager = signInManager;
      _env = env;
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
      if (ModelState.IsValid)
      {
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

        var user = new User
        {
          UserName = model.Username,
          Email = model.Email,
          Name = model.Name,
          JoinDate = DateTime.UtcNow,
          DOB = model.DOB
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
          await _userManager.AddToRoleAsync(user, "Customer");
          user.RoleNames = await _userManager.GetRolesAsync(user);
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
      }

      return View(model);
    }

    [HttpGet]
    public IActionResult Login(string returnUrl = "")
    {
      var model = new LoginViewModel { ReturnUrl = returnUrl };
      return View(model);
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
        var signedInUser = await _userManager.FindByNameAsync(model.Username);
        if (signedInUser != null)
        {
          signedInUser.RoleNames = await _userManager.GetRolesAsync(signedInUser);
          return await RedirectToDashboardByRole(signedInUser);
        }
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
      var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { returnUrl });
      var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
      return Challenge(properties, provider);
    }

    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
      returnUrl ??= Url.Content("~/");

      if (remoteError != null)
      {
        ModelState.AddModelError("", $"Error from external provider: {remoteError}");
        return RedirectToAction(nameof(Login));
      }

      var info = await _signInManager.GetExternalLoginInfoAsync();
      if (info == null)
      {
        ModelState.AddModelError("", "External login info is null.");
        return RedirectToAction(nameof(Login));
      }

      var result = await _signInManager.ExternalLoginSignInAsync(
          info.LoginProvider, info.ProviderKey, isPersistent: false);

      if (result.Succeeded)
      {
        var existingUser = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        if (existingUser != null)
        {
          return await RedirectToDashboardByRole(existingUser);
        }

        return Redirect(returnUrl);
      }

      var email = info.Principal?.FindFirstValue(ClaimTypes.Email);
      var fullName = info.Principal?.FindFirstValue(ClaimTypes.Name) ?? email ?? "Google User";

      if (string.IsNullOrWhiteSpace(email))
      {
        ModelState.AddModelError("", "Google account missing email claim.");
        return RedirectToAction(nameof(Login));
      }

      var newUser = new User
      {
        UserName = email,
        Email = email,
        Name = fullName,
        JoinDate = DateTime.UtcNow
      };

      var createResult = await _userManager.CreateAsync(newUser);
      if (createResult.Succeeded)
      {
        await _userManager.AddLoginAsync(newUser, info);
        await _userManager.AddToRoleAsync(newUser, "Customer");
        newUser.RoleNames = await _userManager.GetRolesAsync(newUser);
        await _signInManager.SignInAsync(newUser, isPersistent: false);
        return await RedirectToDashboardByRole(newUser);
      }

      foreach (var error in createResult.Errors)
      {
        ModelState.AddModelError("", error.Description);
      }

      return RedirectToAction(nameof(Login));
    }


    public IActionResult AccessDenied() => View();

    [HttpGet]
    public IActionResult ChangePassword()
    {
      var model = new ChangePasswordViewModel
      {
        Username = User.Identity?.Name ?? ""
      };
      return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
      if (ModelState.IsValid)
      {
        var user = await _userManager.FindByNameAsync(model.Username);
        if (user == null)
        {
          ModelState.AddModelError("", "User not found.");
          return View(model);
        }

        var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
        if (result.Succeeded)
        {
          TempData["message"] = "Password changed successfully.";
          return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
          ModelState.AddModelError("", error.Description);
      }

      return View(model);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> ViewProfile()
    {
      var user = await _userManager.GetUserAsync(User);
      if (user == null) return NotFound();

      var roles = await _userManager.GetRolesAsync(user);

      var vm = new EditProfileViewModel
      {
        UserName = user.UserName ?? "",
        Name = user.Name,
        Email = user.Email ?? string.Empty,
        DOB = user.DOB,
        ProfileImageUrl = string.IsNullOrEmpty(user.ProfileImageName)
    ? "/uploads/profile/default.png"
    : "/uploads/profile/" + user.ProfileImageName,

        RoleNames = roles
      };

      return View(vm);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
      var user = await _userManager.GetUserAsync(User);
      if (user == null) return NotFound();

      var model = new EditProfileViewModel
      {
        Name = user.Name,
        Email = user.Email,
        DOB = user.DOB,
        ProfileImageUrl = string.IsNullOrEmpty(user.ProfileImageName)
              ? "/uploads/profile/default.png"
              : "/uploads/profile/" + user.ProfileImageName
      };

      return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model)
    {
      var user = await _userManager.GetUserAsync(User);
      if (user == null) return NotFound();

      if (!ModelState.IsValid)
      {
        model.ProfileImageUrl = string.IsNullOrEmpty(user.ProfileImageName)
            ? "/uploads/profile/default.png"
            : "/uploads/profile/" + user.ProfileImageName;
        return View(model);
      }

      if (!string.Equals(model.Email, user.Email, StringComparison.OrdinalIgnoreCase))
      {
        var exists = await _userManager.FindByEmailAsync(model.Email);
        if (exists != null && exists.Id != user.Id)
        {
          ModelState.AddModelError("Email", "This email is already taken.");
          model.ProfileImageUrl = string.IsNullOrEmpty(user.ProfileImageName)
              ? "/uploads/profile/default.png"
              : "/uploads/profile/" + user.ProfileImageName;
          return View(model);
        }
      }

      user.Name = model.Name;
      user.Email = model.Email;
      user.DOB = model.DOB;

      if (model.ProfileImageFile != null && model.ProfileImageFile.Length > 0)
      {
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp" };
        var contentType = model.ProfileImageFile.ContentType.ToLower();

        if (!allowedTypes.Contains(contentType))
        {
          ModelState.AddModelError("", "Only image files (JPG, PNG, GIF, BMP, WEBP) are allowed.");
          model.ProfileImageUrl = string.IsNullOrEmpty(user.ProfileImageName)
              ? "/uploads/profile/default.png"
              : "/uploads/profile/" + user.ProfileImageName;
          return View(model);
        }

        var folder = Path.Combine(_env.WebRootPath, "uploads", "profile");
        Directory.CreateDirectory(folder);

        if (!string.IsNullOrEmpty(user.ProfileImageName) && user.ProfileImageName != "default.png")
        {
          var oldPath = Path.Combine(folder, user.ProfileImageName);
          if (System.IO.File.Exists(oldPath))
            System.IO.File.Delete(oldPath);
        }

        var uniqueFile = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfileImageFile.FileName);
        var filePath = Path.Combine(folder, uniqueFile);

        using var stream = new FileStream(filePath, FileMode.Create);
        await model.ProfileImageFile.CopyToAsync(stream);

        user.ProfileImageName = uniqueFile;
      }

      if (!string.IsNullOrWhiteSpace(model.Password))
      {
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, model.Password);

        if (!result.Succeeded)
        {
          foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);

          model.ProfileImageUrl = string.IsNullOrEmpty(user.ProfileImageName)
              ? "/uploads/profile/default.png"
              : "/uploads/profile/" + user.ProfileImageName;
          return View(model);
        }
      }

      await _userManager.UpdateAsync(user);
      return RedirectToAction("ViewProfile");
    }
  }
}