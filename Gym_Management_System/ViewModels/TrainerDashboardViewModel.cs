using GymManagement.Models;


namespace GymManagement.ViewModels
{
  public class TrainerDashboardViewModel
  {
    public int TotalSessions { get; set; }
    public int TodaySessionsCount { get; set; }
    public int TotalGymClasses { get; set; }

    public List<Session> UpcomingSessions { get; set; } = new();
  }
}
