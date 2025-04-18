using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManagement.Data;
using GymManagement.Models;
using GymManagementSystem.ViewModels;


public class GymBranchController : Controller
{
  private readonly AppDbContext _dbContext;

  public GymBranchController(AppDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  // ✅ 所有人都可以访问
  [AllowAnonymous]
  public IActionResult Index()
  {
    var branches = _dbContext.GymBranches.Select(b => new GymBranchViewModel
    {
      BranchId = b.BranchId,
      BranchName = b.BranchName,
      Address = b.Address,
      ContactNumber = b.ContactNumber,
      TrainerCount = _dbContext.Trainers.Count(t => t.BranchId == b.BranchId),
      ReceptionistCount = _dbContext.Receptionists.Count(r => r.BranchId == b.BranchId),
      RoomCount = _dbContext.Rooms.Count(r => r.BranchId == b.BranchId),
      ImageUrl = "/img/branches/branch" + b.BranchId + ".png"
    }).ToList();

    return View(branches);
  }

  // ✅ 所有人都可以访问
  // GymBranchController.cs
  [AllowAnonymous]


  public IActionResult Details(int id)
  {
    var branch = _dbContext.GymBranches
        .Include(b => b.Trainers)
            .ThenInclude(t => t.GymClasses)
        .Include(b => b.Trainers)
            .ThenInclude(t => t.Sessions)
        .FirstOrDefault(b => b.BranchId == id);

    if (branch == null) return NotFound();

    var sessions = _dbContext.Sessions
        .Include(s => s.GymClass)
        .Include(s => s.Trainer)
        .Where(s => s.Trainer.GymBranch.BranchId == id)
        .ToList();

    var viewModel = new GymBranchDetailsViewModel
    {
      BranchId = branch.BranchId,
      BranchName = branch.BranchName,
      Address = branch.Address,
      ContactNumber = branch.ContactNumber,
      ImageUrl = $"/img/branches/branch{branch.BranchId}.png", // 你自己定义路径的方式
      Trainers = branch.Trainers.Select(t => new TrainerWithClassesViewModel
      {
        Name = t.Name,
        ImageUrl = t.ProfileImageName,
        ClassNames = t.GymClasses.Select(c => c.ClassName).ToList()
      }).ToList(),
      Sessions = sessions.Select(s => new BranchSessionDisplayViewModel
      {
        DayOfWeek = s.SessionDateTime.DayOfWeek.ToString(),
        StartTime = s.SessionDateTime.ToString("hh\\:mm"),
        EndTime = s.SessionDateTime.AddHours(1).ToString("hh\\:mm"), // 假设每节课 1 小时
        ClassName = s.GymClass.ClassName,
        TrainerName = s.Trainer.Name
      }).ToList(),
      Rooms = _dbContext.Rooms
        .Where(r => r.BranchId == branch.BranchId)
        .ToList()

    };

    return View(viewModel);
  }




  // 🔒 仅管理员可以访问
  [Authorize(Roles = "Admin")]
  [HttpGet]
  public IActionResult Create()
  {
    return View();
  }

  [Authorize(Roles = "Admin")]
  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult Create(GymBranch branch)
  {
    if (ModelState.IsValid)
    {
      _dbContext.GymBranches.Add(branch);
      _dbContext.SaveChanges();
      return RedirectToAction("Index");
    }

    return View(branch);
  }

  // 🔒 编辑
  [Authorize(Roles = "Admin")]
  [HttpGet]
  public IActionResult Edit(int id)
  {
    var branch = _dbContext.GymBranches.FirstOrDefault(b => b.BranchId == id);
    if (branch == null)
    {
      return NotFound("Gym branch not found.");
    }

    return View(branch);
  }

  [Authorize(Roles = "Admin")]
  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult Edit(GymBranch updatedBranch)
  {
    var branch = _dbContext.GymBranches.FirstOrDefault(b => b.BranchId == updatedBranch.BranchId);
    if (branch == null)
    {
      return NotFound("Gym branch not found.");
    }

    branch.BranchName = updatedBranch.BranchName;
    branch.Address = updatedBranch.Address;
    branch.ContactNumber = updatedBranch.ContactNumber;

    _dbContext.SaveChanges();
    return RedirectToAction("Index");
  }

  // 🔒 删除
  [Authorize(Roles = "Admin")]
  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult Delete(int id)
  {
    var branch = _dbContext.GymBranches
        .Include(b => b.Trainers)
        .Include(b => b.Receptionists)
        .Include(b => b.Rooms)
        .FirstOrDefault(b => b.BranchId == id);

    if (branch == null)
    {
      return NotFound("Gym branch not found.");
    }

    if (branch.Trainers.Any() || branch.Receptionists.Any() || branch.Rooms.Any())
    {
      ViewBag.Error = "Cannot delete a branch that has associated trainers, receptionists, or rooms.";
      return RedirectToAction("Index");
    }

    _dbContext.GymBranches.Remove(branch);
    _dbContext.SaveChanges();
    return RedirectToAction("Index");
  }
}
