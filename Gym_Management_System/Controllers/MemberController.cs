using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManagement.Data;
using GymManagement.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;


[Authorize(Roles = "Customer")] // Only allow access to Customers
public class MemberController : Controller
{
  private readonly AppDbContext _dbContext;
  private readonly UserManager<User> _userManager;

  public MemberController(AppDbContext dbContext, UserManager<User> userManager)
  {
      _dbContext = dbContext;
      _userManager = userManager;
  }

  private Customer? GetCurrentCustomer()
  {
    string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId)) return null;

    return _dbContext.Customers
      .Include(c => c.Bookings)
        .ThenInclude(b => b.Session)
          .ThenInclude(s => s.GymClass)
      .Include(c => c.Payments)
      .FirstOrDefault(c => c.Id == userId);
  }

  // 🔹 Member Dashboard
  public IActionResult Dashboard()
  {
    var customer = GetCurrentCustomer();
    if (customer == null) return NotFound("Customer not found.");
    return View(customer);
  }

  // 🔹 Book a Gym Session
  [HttpPost]
  public IActionResult BookSession(int sessionId)
  {
    var customer = GetCurrentCustomer();
    if (customer == null) return NotFound("Customer not found.");

    var session = _dbContext.Sessions.Find(sessionId);
    if (session == null) return NotFound("Session not found.");

    bool alreadyBooked = _dbContext.Bookings.Any(b => b.CustomerId == customer.Id && b.SessionId == sessionId);
    if (alreadyBooked)
    {
      TempData["Error"] = "You have already booked this session.";
      return RedirectToAction("Dashboard");
    }

    var booking = new Booking
    {
      CustomerId = customer.Id,
      SessionId = sessionId,
      BookingDate = DateTime.UtcNow,
      Status = BookingStatus.Pending
    };

    _dbContext.Bookings.Add(booking);
    _dbContext.SaveChanges();

    return RedirectToAction("Dashboard");
  }

  // 🔹 Cancel a Booking
  [HttpPost]
  public IActionResult CancelBooking(int bookingId)
  {
    var customer = GetCurrentCustomer();
    if (customer == null) return NotFound("Customer not found.");

    var booking = _dbContext.Bookings
      .FirstOrDefault(b => b.BookingId == bookingId && b.CustomerId == customer.Id);

    if (booking == null) return NotFound("Booking not found.");

    _dbContext.Bookings.Remove(booking);
    _dbContext.SaveChanges();

    return RedirectToAction("Dashboard");
  }

  // 🔹 Membership Status
  public IActionResult MembershipStatus()
  {
    var customer = GetCurrentCustomer();
    if (customer == null) return NotFound("Customer not found.");
    return View(customer);
  }

  // 🔹 Workout History
  public IActionResult WorkoutHistory()
  {
    var customer = GetCurrentCustomer();
    if (customer == null) return NotFound("Customer not found.");

    var history = _dbContext.Bookings
      .Include(b => b.Session)
        .ThenInclude(s => s.GymClass)
      .Where(b => b.CustomerId == customer.Id && b.Status == BookingStatus.CheckedIn)
      .OrderByDescending(b => b.BookingDate)
      .ToList();

    return View(history);
  }

  // 🔹 View Profile
  [Route("Account/ViewProfile")]
  public async Task<IActionResult> Profile()
  {
      var customer = GetCurrentCustomer();
      if (customer == null) return NotFound("Customer not found.");
      var roles = await _userManager.GetRolesAsync(customer);
      var viewModel = new EditProfileViewModel
      {
          UserName = customer.UserName,
          Name = customer.Name,
          Email = customer.Email,
          DOB = customer.DOB,
          ProfileImageUrl = string.IsNullOrEmpty(customer.ProfileImageName)
              ? "/uploads/profile/default.png"
              : "/uploads/profile/" + customer.ProfileImageName,
          RoleNames = roles.ToList()
      };

      return View("~/Views/Account/ViewProfile.cshtml", viewModel);
  }
  // GET: /Member/EditProfile
  [HttpGet]
  public async Task<IActionResult> EditProfile()
  {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userId)) return NotFound("User not logged in.");

      var user = await _userManager.FindByIdAsync(userId);
      if (user == null) return NotFound("User not found.");

      var customer = _dbContext.Customers.FirstOrDefault(c => c.Id == userId);
      if (customer == null) return NotFound("Customer not found.");

      var roles = await _userManager.GetRolesAsync(user);

      var viewModel = new EditProfileViewModel
      {
          UserName = user.UserName,
          Name = customer.Name,
          Email = customer.Email,
          DOB = user.DOB,
          ProfileImageUrl = string.IsNullOrEmpty(user.ProfileImageName)
              ? "/uploads/profile/default.png"
              : "/uploads/profile/" + user.ProfileImageName,
          RoleNames = roles.ToList()
      };

      return View("~/Views/Account/EditProfile.cshtml", viewModel);
  }

  // 🔹 Edit Profile
  [HttpPost]
  public async Task<IActionResult> EditProfile(EditProfileViewModel model)
  {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userId)) return NotFound("User not logged in.");

      var user = await _userManager.FindByIdAsync(userId);
      if (user == null) return NotFound("User not found.");

      var customer = _dbContext.Customers.FirstOrDefault(c => c.Id == userId);
      if (customer == null) return NotFound("Customer not found.");
      if (model.ProfileImageFile != null && model.ProfileImageFile.Length > 0)
      {
          var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/profile");
          Directory.CreateDirectory(uploadsDir); 

          var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ProfileImageFile.FileName)}";
          var filePath = Path.Combine(uploadsDir, fileName);

          using (var stream = new FileStream(filePath, FileMode.Create))
          {
              await model.ProfileImageFile.CopyToAsync(stream);
          }

          user.ProfileImageName = fileName;
      }
      user.Name = model.Name;
      user.Email = model.Email;
      user.DOB = model.DOB;
      customer.Name = model.Name;
      customer.Email = model.Email;
      if (!string.IsNullOrWhiteSpace(model.Password))
      {
          var token = await _userManager.GeneratePasswordResetTokenAsync(user);
          var result = await _userManager.ResetPasswordAsync(user, token, model.Password);
          if (!result.Succeeded)
          {
              foreach (var error in result.Errors)
              {
                  ModelState.AddModelError("", error.Description);
              }
              return View("~/Views/Account/EditProfile.cshtml", model); 
          }
      }
      await _userManager.UpdateAsync(user);
      _dbContext.SaveChanges();

      return RedirectToAction("Profile");
  }
}