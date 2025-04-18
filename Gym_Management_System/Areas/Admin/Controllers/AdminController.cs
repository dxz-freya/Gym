using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GymManagement.Data;
using GymManagement.Models;
using GymManagement.Areas.Admin.Models;

namespace GymManagement.Areas.Admin.Controllers
{
  [Area("Admin")]
  [Authorize(Roles = "Admin")]
  public class AdminController : Controller
  {
    private readonly AppDbContext _dbContext;
    private readonly UserManager<User> _userManager;

    public AdminController(AppDbContext dbContext, UserManager<User> userManager)
    {
      _dbContext = dbContext;
      _userManager = userManager;
    }

    public async Task<IActionResult> Dashboard()
    {
      var today = DateTime.Today;

      var sessions = await _dbContext.Sessions
          .Include(s => s.Trainer).ThenInclude(t => t.GymBranch)
          .Include(s => s.Room)
          .Include(s => s.GymClass)
          .Include(s => s.Bookings)
          .Where(s => s.SessionDateTime.Date >= today)
          .ToListAsync();

      var sessionsByDate = sessions
          .GroupBy(s => s.SessionDateTime.Date)
          .OrderBy(g => g.Key)
          .ToDictionary(
              g => g.Key,
              g => g.Select(s => new Session
              {
                SessionId = s.SessionId,
                SessionName = s.SessionName,
                SessionDateTime = s.SessionDateTime,
                Trainer = s.Trainer,
                Room = s.Room,
                GymClass = s.GymClass,
                Category = s.Category,
                Capacity = s.Capacity,
                Bookings = s.Bookings
              }).ToList()
          );

      var viewModel = new AdminDashboardViewModel
      {
        TotalSessions = await _dbContext.Sessions.CountAsync(),
        TotalMembers = await _dbContext.Users.OfType<Customer>().CountAsync(),
        TotalTrainers = await _dbContext.Users.OfType<Trainer>().CountAsync(),
        TotalBranches = await _dbContext.GymBranches.CountAsync(),
        UpcomingSessionsByDate = sessionsByDate
      };

      return View(viewModel);
    }


    [HttpGet]
    public IActionResult CreateSession(DateTime? date)
    {
      var vm = new CreateSessionViewModel
      {
        // ✅ If a date is provided, default time is 09:00 AM that day; otherwise empty
        SessionDateTime = date.HasValue ? date.Value.Date.AddHours(9) : DateTime.MinValue,

        // ✅ Dropdown: Gym class templates
        GymClassList = _dbContext.GymClasses
              .Select(c => new SelectListItem { Text = c.ClassName, Value = c.GymClassId.ToString() })
              .ToList(),

        // ✅ Dropdown: Trainers
        TrainerList = _dbContext.Users
              .OfType<Trainer>()
              .Select(t => new SelectListItem { Text = t.Name, Value = t.Id })
              .ToList(),

        // ✅ Dropdown: Rooms
        RoomList = _dbContext.Rooms
              .Select(r => new SelectListItem { Text = r.RoomName, Value = r.RoomId.ToString() })
              .ToList(),

        // ✅ Dropdown: Session categories (e.g. Yoga, HIIT)
        CategoryList = Enum.GetValues(typeof(SessionCategory))
              .Cast<SessionCategory>()
              .Select(c => new SelectListItem
              {
                Text = c.ToString(),
                Value = c.ToString()
              }).ToList(),

        // ✅ Default branch label (readonly field)
        BranchName = "N/A"
      };

      return View(vm);
    }


    [HttpPost]
    public async Task<IActionResult> CreateSession(CreateSessionViewModel vm)
    {
      if (vm.SessionDateTime <= DateTime.Now)
      {
        ModelState.AddModelError("SessionDateTime", "Session time must be in the future.");
      }

      if (!ModelState.IsValid)
      {
        vm.GymClassList = _dbContext.GymClasses
            .Select(c => new SelectListItem { Text = c.ClassName, Value = c.GymClassId.ToString() })
            .ToList();
        vm.TrainerList = _dbContext.Users
            .OfType<Trainer>()
            .Select(t => new SelectListItem { Text = t.Name, Value = t.Id })
            .ToList();
        vm.RoomList = _dbContext.Rooms
            .Select(r => new SelectListItem { Text = r.RoomName, Value = r.RoomId.ToString() })
            .ToList();
        vm.CategoryList = Enum.GetValues(typeof(SessionCategory))
            .Cast<SessionCategory>()
            .Select(c => new SelectListItem
            {
              Text = c.ToString(),
              Value = c.ToString()
            }).ToList();
        vm.BranchName = null;
        return View(vm);
      }

      var currentUser = await _userManager.GetUserAsync(User);
      if (currentUser == null)
      {
        return Forbid();
      }

      var session = new Session
      {
        SessionDateTime = vm.SessionDateTime,
        GymClassId = vm.GymClassId,
        TrainerId = vm.TrainerId,
        RoomId = vm.RoomId,
        Capacity = vm.Capacity,
        Category = vm.Category,
        SessionName = vm.SessionName,
        ReceptionistId = currentUser.Id
      };

      _dbContext.Sessions.Add(session);
      await _dbContext.SaveChangesAsync();
      return RedirectToAction("Dashboard");
    }

    [HttpGet]
    public IActionResult GetRoomCapacity(int roomId)
    {
      var capacity = _dbContext.Rooms.Where(r => r.RoomId == roomId)
          .Select(r => r.Capacity)
          .FirstOrDefault();

      return Json(new { capacity });
    }

    public IActionResult ManageMembers()
    {
      var members = _dbContext.Customers.ToList();
      ViewBag.Error = TempData["Error"];
      return View(members);
    }

    public IActionResult ManageTrainers()
    {
      var trainers = _dbContext.Trainers.ToList();
      ViewBag.Error = TempData["Error"];
      return View(trainers);
    }

    public IActionResult ManageSessions()
    {
      var sessions = _dbContext.Sessions
          .Include(s => s.GymClass)
          .Include(s => s.Trainer)
          .Include(s => s.Room)
          .OrderBy(s => s.SessionDateTime)
          .ToList();

      return View(sessions);
    }

    public IActionResult ManageGymBranches()
    {
      var branches = _dbContext.GymBranches.ToList();
      return View(branches);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string userId)
    {
      var user = await _userManager.FindByIdAsync(userId);
      if (user == null)
        return NotFound("User not found.");

      if (await _userManager.IsInRoleAsync(user, "Customer"))
      {
        var hasBookings = _dbContext.Bookings.Any(b => b.CustomerId == userId);
        var hasPayments = _dbContext.Payments.Any(p => p.CustomerId == userId);
        if (hasBookings || hasPayments)
        {
          TempData["Error"] = "Cannot delete a customer who has existing bookings or payments.";
          return RedirectToAction("ManageMembers");
        }
      }
      else if (await _userManager.IsInRoleAsync(user, "Trainer"))
      {
        var hasSessions = _dbContext.Sessions.Any(s => s.TrainerId == userId);
        if (hasSessions)
        {
          TempData["Error"] = "Cannot delete a trainer who is assigned to sessions.";
          return RedirectToAction("ManageTrainers");
        }
      }

      var result = await _userManager.DeleteAsync(user);
      if (!result.Succeeded)
        TempData["Error"] = "Failed to delete user.";

      bool isCustomer = await _userManager.IsInRoleAsync(user, "Customer");
      return RedirectToAction(isCustomer ? "ManageMembers" : "ManageTrainers");
    }

    [HttpGet]
    public IActionResult GetTrainerBranch(string trainerId)
    {
      var branch = _dbContext.Users
          .OfType<Trainer>()
          .Include(t => t.GymBranch)
          .Where(t => t.Id == trainerId)
          .Select(t => t.GymBranch != null ? t.GymBranch.BranchName : "Unknown")
          .FirstOrDefault();

      return Json(new { branch });
    }

    [HttpGet]
    public IActionResult GetTrainersByClass(int gymClassId)
    {
      var trainers = _dbContext.GymClasses
          .Where(c => c.GymClassId == gymClassId)
          .Select(c => c.Trainer)
          .Distinct()
          .Select(t => new { t.Id, t.Name })
          .ToList();

      return Json(trainers);
    }

    [HttpGet]
    public IActionResult GetRoomsByTrainer(string trainerId)
    {
      var branchId = _dbContext.Users
          .OfType<Trainer>()
          .Where(t => t.Id == trainerId)
          .Select(t => t.BranchId)
          .FirstOrDefault();

      var rooms = _dbContext.Rooms
          .Where(r => r.BranchId == branchId)
          .Select(r => new { r.RoomId, r.RoomName, r.Capacity })
          .ToList();

      return Json(rooms);
    }


  }
}
