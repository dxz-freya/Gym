using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManagement.Data;
using GymManagement.Models;
using System.Security.Claims;

public class ClassScheduleController : Controller
{
  private readonly AppDbContext _dbContext;

  public ClassScheduleController(AppDbContext context)
  {
    _dbContext = context;
  }

  // 🔹 Display all available class sessions
  public IActionResult Index()
  {
    var sessions = _dbContext.Sessions
        .Include(s => s.GymClass)
        .Include(s => s.Trainer)
        .Include(s => s.Room)
        .OrderBy(s => s.SessionDateTime)
        .ToList();

    ViewBag.Error = TempData["Error"];
    return View(sessions);
  }

  // 🔹 Session details
  public IActionResult SessionDetails(int sessionId)
  {
    var session = _dbContext.Sessions
        .Include(s => s.GymClass)
        .Include(s => s.Trainer)
        .Include(s => s.Room)
        .Include(s => s.Bookings)
            .ThenInclude(b => b.Customer)
        .FirstOrDefault(s => s.SessionId == sessionId);

    if (session == null)
    {
      return NotFound("Session not found.");
    }

    return View(session);
  }

  // 🔹 Book a session
  [Authorize(Roles = "Customer")]
  [HttpPost]
  public IActionResult BookSession(int sessionId)
  {
    string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    if (string.IsNullOrEmpty(userId))
    {
      return BadRequest("Invalid user identifier.");
    }

    var session = _dbContext.Sessions.FirstOrDefault(s => s.SessionId == sessionId);
    if (session == null)
    {
      return NotFound("Session not found.");
    }

    var customer = _dbContext.Customers.FirstOrDefault(c => c.Id == userId);
    if (customer == null)
    {
      return NotFound("Customer not found.");
    }

    var receptionist = _dbContext.Receptionists.FirstOrDefault(r => r.Id == session.ReceptionistId);

    bool alreadyBooked = _dbContext.Bookings.Any(b => b.CustomerId == userId && b.SessionId == sessionId);
    if (alreadyBooked)
    {
      TempData["Error"] = "You have already booked this session.";
      return RedirectToAction("Index");
    }

    var booking = new Booking
    {
      BookingDate = DateTime.UtcNow,
      Status = BookingStatus.Pending,
      CustomerId = userId,
      SessionId = session.SessionId,
      ReceptionistId = receptionist?.Id
    };

    _dbContext.Bookings.Add(booking);
    _dbContext.SaveChanges();

    return RedirectToAction("Index");
  }

  // 🔹 Cancel a booking
  [Authorize(Roles = "Customer")]
  [HttpPost]
  public IActionResult CancelBooking(int bookingId)
  {
    string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    if (string.IsNullOrEmpty(userId))
    {
      return BadRequest("Invalid user identifier.");
    }

    var booking = _dbContext.Bookings.FirstOrDefault(b => b.BookingId == bookingId && b.CustomerId == userId);
    if (booking == null)
    {
      return NotFound("Booking not found.");
    }

    _dbContext.Bookings.Remove(booking);
    _dbContext.SaveChanges();

    return RedirectToAction("Index");
  }
}
