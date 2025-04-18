using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManagement.Data;
using GymManagement.Models;
using GymManagement.ViewModels;
using System.Security.Claims;

namespace GymManagement.Controllers
{
  [Authorize(Roles = "Trainer")]
  public class TrainerController : Controller
  {
    private readonly AppDbContext _dbContext;

    public TrainerController(AppDbContext dbContext)
    {
      _dbContext = dbContext;
    }

    // 🔹 教练仪表盘
    public async Task<IActionResult> Dashboard()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var today = DateTime.Today;

      var sessions = await _dbContext.Sessions
          .Include(s => s.GymClass)
          .Include(s => s.Room)
          .Include(s => s.Bookings)
          .Where(s => s.TrainerId == userId)
          .ToListAsync();

      var gymClasses = await _dbContext.GymClasses
          .Where(c => c.TrainerId == userId)
          .ToListAsync();

      var model = new TrainerDashboardViewModel
      {
        TotalSessions = sessions.Count,
        TodaySessionsCount = sessions.Count(s => s.SessionDateTime.Date == today),
        UpcomingSessions = sessions
              .Where(s => s.SessionDateTime >= today)
              .OrderBy(s => s.SessionDateTime)
              .Take(10)
              .ToList(),
        TotalGymClasses = gymClasses.Count
      };

      return View(model);
    }

    [HttpGet]
public async Task<IActionResult> GetSessionBookings(int sessionId, int page = 1, int pageSize = 5)
{
    var bookings = await _dbContext.Bookings
        .Where(b => b.SessionId == sessionId)
        .Include(b => b.Customer)
        .Include(b => b.Session).ThenInclude(s => s.GymClass)
        .OrderBy(b => b.BookingId)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(b => new
        {
            customerName = b.Customer.Name,
            sessionName = b.Session.GymClass.ClassName,
            sessionTime = b.Session.SessionDateTime.ToString("yyyy-MM-dd HH:mm"),
            status = b.Status.ToString()
        })
        .ToListAsync();

    int totalCount = await _dbContext.Bookings.CountAsync(b => b.SessionId == sessionId);

    return Json(new { bookings, totalCount });
}


    

// 🔹 显示 Trainer 自己的 GymClasses 页面
        [HttpGet]
public IActionResult MyGymClasses()
{
    var trainerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    var gymClasses = _dbContext.GymClasses
        .Where(g => g.TrainerId == trainerId)
        .OrderByDescending(g => g.AvailableTime)
        .Select(g => new GymClassViewModel
        {
            GymClassId = g.GymClassId,
            ClassName = g.ClassName,
            AvailableTime = g.AvailableTime,
            Duration = g.Duration,
            Description = g.Description,
            ImageName = g.ImageName // ✅ 添加这一行
        }).ToList();

    return View(gymClasses);
}


        // 🔹 创建 GymClass（modal 提交）
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> CreateGymClass(GymClassViewModel model)
{
    if (!ModelState.IsValid) return View(model);

    var trainerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var imageName = await SaveImageAsync(model.ImageFile) ?? "class-default.jpg";

    var gymClass = new GymClass
    {
        ClassName = model.ClassName,
        Description = model.Description,
        Duration = model.Duration,
        AvailableTime = model.AvailableTime,
        TrainerId = trainerId,
        ImageName = imageName
    };

    _dbContext.GymClasses.Add(gymClass);
    await _dbContext.SaveChangesAsync();

    return RedirectToAction(nameof(MyGymClasses));
}

        // 🔹 编辑 GymClass（modal 提交）
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> EditGymClass(GymClassViewModel model)
{
    if (!ModelState.IsValid) return View(model);

    var trainerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var gymClass = _dbContext.GymClasses.FirstOrDefault(g =>
        g.GymClassId == model.GymClassId && g.TrainerId == trainerId);

    if (gymClass == null) return NotFound();

    gymClass.ClassName = model.ClassName;
    gymClass.Description = model.Description;
    gymClass.AvailableTime = model.AvailableTime;
    gymClass.Duration = model.Duration;

    var newImage = await SaveImageAsync(model.ImageFile);
    if (!string.IsNullOrEmpty(newImage))
        gymClass.ImageName = newImage;

    await _dbContext.SaveChangesAsync();
    return RedirectToAction(nameof(MyGymClasses));
}


        // 🔹 删除 GymClass（modal 提交）
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteGymClass(int id)
        {
            var trainerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var gymClass = _dbContext.GymClasses.FirstOrDefault(g =>
                g.GymClassId == id && g.TrainerId == trainerId);

            if (gymClass == null) return NotFound();

            _dbContext.GymClasses.Remove(gymClass);
            _dbContext.SaveChanges();
            return RedirectToAction(nameof(MyGymClasses));
        }

    // 🔹 查看教练的所有 Session
    public IActionResult ViewSessions()
    {
      var trainerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

      var sessions = _dbContext.Sessions
          .Include(s => s.GymClass)
          .Include(s => s.Room)
          .Where(s => s.TrainerId == trainerId)
          .OrderBy(s => s.SessionDateTime)
          .ToList();

      return View(sessions);
    }

    // 🔹 查看某个 Session 详情（包括预约列表）
    public IActionResult SessionDetails(int sessionId)
    {
      var trainerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

      var session = _dbContext.Sessions
          .Include(s => s.GymClass)
          .Include(s => s.Room)
          .Include(s => s.Bookings).ThenInclude(b => b.Customer)
          .FirstOrDefault(s => s.SessionId == sessionId && s.TrainerId == trainerId);

      if (session == null) return NotFound("Session not found.");

      return View(session);
    }

    // 🔹 标记考勤
    [HttpPost]
    public IActionResult MarkAttendance(int bookingId)
    {
      var booking = _dbContext.Bookings.FirstOrDefault(b => b.BookingId == bookingId);
      if (booking == null) return NotFound();

      booking.Status = BookingStatus.CheckedIn;
      booking.CheckInTime = DateTime.UtcNow;
      _dbContext.SaveChanges();

      return RedirectToAction("SessionDetails", new { sessionId = booking.SessionId });
    }

    // 🔹 显示 Trainer 编辑信息表单
    [HttpGet]
    public IActionResult EditProfile()
    {
      var trainerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var trainer = _dbContext.Trainers.FirstOrDefault(t => t.Id == trainerId);
      if (trainer == null) return NotFound();

      var model = new EditTrainerProfileViewModel
      {
        TrainerId = trainer.Id,
        Name = trainer.Name,
        Email = trainer.Email,
        Specialization = trainer.Specialization,
        ExperienceStarted = trainer.ExperienceStarted
      };

      return View(model);
    }

    // 🔹 提交 Trainer 编辑信息
    [HttpPost]
    public IActionResult EditProfile(EditTrainerProfileViewModel model)
    {
      if (!ModelState.IsValid) return View(model);

      var trainerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var trainer = _dbContext.Trainers.FirstOrDefault(t => t.Id == trainerId);
      if (trainer == null) return NotFound();

      trainer.Name = model.Name;
      trainer.Email = model.Email;
      trainer.Specialization = string.IsNullOrWhiteSpace(model.Specialization) ? "Unknown" : model.Specialization;
      trainer.ExperienceStarted = model.ExperienceStarted ?? DateTime.Today;

      _dbContext.SaveChanges();
      return RedirectToAction("Dashboard");
    }

    private async Task<string?> SaveImageAsync(IFormFile? imageFile)
{
    if (imageFile == null || imageFile.Length == 0) return null;

    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/gymclass");
    if (!Directory.Exists(uploadsFolder))
        Directory.CreateDirectory(uploadsFolder);

    var extension = Path.GetExtension(imageFile.FileName);
    var fileName = $"{Guid.NewGuid()}{extension}";
    var filePath = Path.Combine(uploadsFolder, fileName);

    using var stream = new FileStream(filePath, FileMode.Create);
    await imageFile.CopyToAsync(stream);

    return fileName;
}


  }
}
