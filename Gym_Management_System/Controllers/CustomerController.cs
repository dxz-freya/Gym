using Microsoft.AspNetCore.Mvc;
using GymManagement.Models;
using GymManagement.Data;
using GymManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace GymManagement.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<User> _userManager;

        public CustomerController(AppDbContext dbContext, UserManager<User> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        private async Task<Customer> GetCurrentCustomerAsync()
        {
            var userId = _userManager.GetUserId(User);
            var customer = await _dbContext.Customers
                .Include(c => c.Bookings)
                    .ThenInclude(b => b.Session)
                        .ThenInclude(s => s.Room)
                            .ThenInclude(r => r.GymBranch)
                .Include(c => c.Bookings)
                    .ThenInclude(b => b.Session)
                        .ThenInclude(s => s.Trainer)
                .Include(c => c.Bookings)
                    .ThenInclude(b => b.Session)
                        .ThenInclude(s => s.GymClass)
                .Include(c => c.Payments)
                .FirstOrDefaultAsync(c => c.Id == userId);

            return customer!;
        }

        public async Task<IActionResult> Dashboard()
        {
            var customer = await GetCurrentCustomerAsync();

            var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);

            var upcomingBookings = customer.Bookings
                .Where(b => b.Session != null && b.Session.SessionDateTime > DateTime.Now)
                .Select(b => new BookingViewModel
                {
                    BookingId = b.BookingId,
                    SessionName = b.Session?.SessionName,
                    ClassName = b.Session?.GymClass?.ClassName ?? "N/A",
                    SessionDate = b.Session!.SessionDateTime,
                    BookingDate = b.BookingDate,
                    TrainerName = b.Session?.Trainer?.Name ?? "N/A",
                    BranchName = b.Session?.Room?.GymBranch?.BranchName ?? "N/A",
                    RoomName = b.Session?.Room?.RoomName ?? "N/A",
                    Status = b.Status.ToString()
                }).ToList();

            var weeklyActivity = customer.Bookings
                .Where(b => b.Session != null && b.Session.SessionDateTime >= startOfWeek && b.Session.SessionDateTime < endOfWeek)
                .GroupBy(b => b.Session.SessionDateTime.DayOfWeek)
                .ToDictionary(g => g.Key, g => g.Count());

            var dashboardVM = new CustomerDashboardViewModel
            {
                Name = customer.Name,
                MembershipStatus = customer.MembershipStatus,
                MembershipType = customer.MembershipType,
                MembershipExpiry = customer.MembershipExpiry,
                SubscriptionDate = customer.SubscriptionDate,
                Payments = customer.Payments.Select(p => new PaymentViewModel
                {
                    Price = p.Price,
                    PaymentMethod = p.PaymentMethod,
                    PaymentDate = p.PaymentDate
                }).ToList(),
                Bookings = upcomingBookings,
                UpcomingBookings = upcomingBookings,
                WeeklyActivity = weeklyActivity
            };

            return View("Dashboard", dashboardVM);
        }

        public async Task<IActionResult> Profile()
        {
            var customer = await GetCurrentCustomerAsync();
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(Customer updatedCustomer)
        {
            var customer = await GetCurrentCustomerAsync();

            if (!ModelState.IsValid)
                return View(updatedCustomer);

            customer.Name = updatedCustomer.Name;
            customer.PhoneNumber = updatedCustomer.PhoneNumber;

            _dbContext.Update(customer);
            await _dbContext.SaveChangesAsync();

            TempData["Success"] = "Profile updated!";
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> Membership()
        {
            var customer = await GetCurrentCustomerAsync();
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> RenewMembership([FromForm] string returnTo)
        {
            var customer = await GetCurrentCustomerAsync();
            customer.MembershipStatus = "Active";
            customer.MembershipExpiry = (customer.MembershipExpiry ?? DateTime.Now).AddYears(1);

            _dbContext.Update(customer);
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "1 year renewed";

            return returnTo == "Membership"
                ? RedirectToAction("Membership")
                : RedirectToAction("Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(int bookingId)
        {
            var booking = await _dbContext.Bookings.FindAsync(bookingId);

            if (booking == null)
                return NotFound();

            if (booking.Status == BookingStatus.CheckedIn)
            {
                TempData["Message"] = "You have already checked in.";
                return RedirectToAction("Dashboard", "Customer");
            }

            booking.CheckInTime = DateTime.Now;
            booking.Status = BookingStatus.CheckedIn;

            _dbContext.Update(booking);
            await _dbContext.SaveChangesAsync();

            TempData["Message"] = "Check-in successful!";
            return RedirectToAction("Dashboard", "Customer");
        }

        [HttpPost]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            var booking = await _dbContext.Bookings.FindAsync(bookingId);
            if (booking == null) return NotFound();

            booking.Status = BookingStatus.Canceled;
            _dbContext.Update(booking);
            await _dbContext.SaveChangesAsync();

            TempData["Message"] = "Booking canceled successfully!";
            return RedirectToAction("Dashboard");
        }
    }
}
