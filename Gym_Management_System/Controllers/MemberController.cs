using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManagement.Data;
using GymManagement.Models;
using System.Security.Claims;

[Authorize(Roles = "Customer")] // Only allow access to Customers
public class MemberController : Controller
{
  private readonly AppDbContext _dbContext;

  public MemberController(AppDbContext dbContext)
  {
    _dbContext = dbContext;
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
  public IActionResult Profile()
  {
    var customer = GetCurrentCustomer();
    if (customer == null) return NotFound("Customer not found.");
    return View(customer);
  }

  // 🔹 Edit Profile
  [HttpPost]
  public IActionResult EditProfile(Customer updatedCustomer)
  {
    var customer = GetCurrentCustomer();
    if (customer == null) return NotFound("Customer not found.");

    customer.Name = updatedCustomer.Name;
    customer.Email = updatedCustomer.Email;
    customer.MembershipType = updatedCustomer.MembershipType;

    _dbContext.SaveChanges();
    return RedirectToAction("Profile");
  }
}
