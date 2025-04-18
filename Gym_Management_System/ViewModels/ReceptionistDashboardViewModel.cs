using GymManagement.Models;

namespace GymManagement.ViewModels
{
    public class ReceptionistDashboardViewModel
    {
        public int TodayTotalBookings { get; set; }

        public int TodayCheckedIn { get; set; }

        public List<Session> TodaySessions { get; set; } = new();

        public List<Booking> CheckInBookings { get; set; } = new();
    }
}
