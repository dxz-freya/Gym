using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using GymManagement.Data;
using GymManagement.Models;
using GymManagement.ViewModels;

namespace GymManagement.Controllers
{
    [Authorize(Roles = "Customer")]
    public class BookingController : Controller
    {
        private readonly AppDbContext _dbContext;

        public BookingController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        public async Task<IActionResult> BookSession(string? branch, string? search)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var allSessions = await _dbContext.Sessions
                .Include(s => s.Room).ThenInclude(r => r.GymBranch)
                .Include(s => s.Trainer)
                .Include(s => s.Bookings)
                .Where(s => s.SessionDateTime > DateTime.Now)
                .ToListAsync();

            var sessions = allSessions
                .Where(s =>
                    (string.IsNullOrEmpty(branch) || s.Room?.GymBranch?.BranchName == branch) &&
                    (string.IsNullOrEmpty(search) || s.SessionName!.ToLower().Contains(search.ToLower()))
                )
                .Select(s => new ClassScheduleViewModel
                {
                    SessionId = s.SessionId,
                    ClassName = s.SessionName ?? "",
                    TrainerName = s.Trainer?.Name ?? "",
                    RoomName = s.Room?.RoomName ?? "",
                    BranchName = s.Room?.GymBranch?.BranchName ?? "",
                    SessionDateTime = s.SessionDateTime,
                    Capacity = s.Room?.Capacity ?? 0,
                    BookedCount = s.Bookings.Count,
                    IsBookedByUser = s.Bookings.Any(b => b.CustomerId == userId)
                })
                .OrderBy(s => s.SessionDateTime)
                .ToList();

            ViewBag.SelectedBranch = branch;
            ViewBag.SearchTerm = search;
            ViewBag.BookedSessionIds = sessions.Where(s => s.IsBookedByUser).Select(s => s.SessionId).ToList();
            ViewBag.Branches = await _dbContext.GymBranches.Select(b => b.BranchName).ToListAsync();

            return View(sessions);
        }

        [HttpPost]
        public async Task<IActionResult> BookSession(int sessionId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var alreadyBooked = await _dbContext.Bookings
                .AnyAsync(b => b.CustomerId == userId && b.SessionId == sessionId);

            if (alreadyBooked)
            {
                TempData["Error"] = "You have already booked this session.";
                return RedirectToAction("BookSession");
            }

            var booking = new Booking
            {
                SessionId = sessionId,
                CustomerId = userId,
                UserId = userId,
                BookingDate = DateTime.UtcNow,
                Status = BookingStatus.Confirmed
            };

            _dbContext.Bookings.Add(booking);
            await _dbContext.SaveChangesAsync();

            TempData["Success"] = "Session booked successfully!";
            return RedirectToAction("BookSession");
        }
    }
}