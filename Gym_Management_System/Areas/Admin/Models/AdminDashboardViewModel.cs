using System;
using System.Collections.Generic;
using GymManagement.Models;

namespace GymManagement.Areas.Admin.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalSessions { get; set; }
        public int TotalMembers { get; set; }
        public int TotalTrainers { get; set; }
        public int TotalBranches { get; set; }

        // Key: Session Date (yyyy-MM-dd), Value: List of Sessions on that date
        public Dictionary<DateTime, List<Session>> UpcomingSessionsByDate { get; set; } = new();
    }
}
